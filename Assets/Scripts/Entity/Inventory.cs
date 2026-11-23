using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using Newtonsoft.Json;
using System;

public class Inventory : MonoBehaviour
{
    public static Inventory _Instance;
    [SerializeField]
    private int currentMoney = 0;
    [SerializeField]
    private List<Item> items;
    public Text moneyText;
    /// <summary>The premium currency: it pays the summons.</summary>
    [SerializeField]
    private int currentGems = 0;
    private Text gemsText;

    public static Inventory Instance
    {
        get
        {
            if (!_Instance)
            {
                // NOTE: read docs to see directory requirements for Resources.Load!
                var prefab = Resources.Load<GameObject>("Prefabs/System/Inventory");
                // create the prefab in your scene
                var inScene = Instantiate<GameObject>(prefab);
                // try find the instance inside the prefab
                _Instance = inScene.GetComponentInChildren<Inventory>();
                // guess there isn't one, add one
                if (!_Instance) _Instance = inScene.AddComponent<Inventory>();
                // mark root as DontDestroyOnLoad();
                DontDestroyOnLoad(_Instance.transform.root.gameObject);
            }
            return _Instance;
        }
    }

    private void Start()
    {
        //lancer une tache de récupération des items du user
        //NetworkManager.Instance.AddRequest(new NetworkRequest(NetworkRequest.GET_USER_ITEMS));
    }

    public void UpdateMoneyUI()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        if (moneyText == null)
        {
            moneyText = FindCounter("MoneyText", true);
        }
        moneyText.text = currentMoney.ToString("#,0", nfi);
    }

    /// <summary>Shows the gems in the counter of the HUD under the money (the red gem), when the scene has one.</summary>
    public void UpdateGemsUI()
    {
        if (gemsText == null)
        {
            gemsText = FindCounter("DiamondsText", false);
        }
        if (gemsText != null) gemsText.text = UIBuilder.Number(currentGems);
    }

    /// <summary>
    /// The counters of the HUD all have the tag MoneyText: the money and the gems are told apart by their name.
    /// orAny gives the first one when none has this name.
    /// </summary>
    private static Text FindCounter(string name, bool orAny)
    {
        GameObject[] counters = GameObject.FindGameObjectsWithTag("MoneyText");
        foreach (GameObject counter in counters)
        {
            if (counter.name == name) return counter.GetComponent<Text>();
        }
        return orAny && counters.Length > 0 ? counters[0].GetComponent<Text>() : null;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (SceneManager.GetActiveScene().name == "MainScene") UpdateMoneyUI();
    }

    public void SubtractMoney(int amount)
    {
        currentMoney -= amount;
        if (SceneManager.GetActiveScene().name == "MainScene") UpdateMoneyUI();
    }

    public void AddItem(Item item)
    {
        this.items.Add(item);
    }

    /// <summary>Forgets the items before they are loaded again (a new login, e.g. after a maintenance).</summary>
    public void ClearItems()
    {
        this.items.Clear();
    }

    public void RemoveItemsAndPersist(List<Item> itemsToRemove)
    {
        List<ItemAPI> listItemApi = new List<ItemAPI>();
        foreach (Item item in itemsToRemove)
        {
            ItemAPI itemApi = new ItemAPI(item);
            listItemApi.Add(itemApi);
            items.Remove(item);
        }

        string itemsJson = JsonConvert.SerializeObject(listItemApi);
        StartCoroutine(RemoveUserItems(itemsJson));
    }

    IEnumerator RemoveUserItems(string items)
    {
        Loader.instance.SetLoading(true);
        var apiEndPoint = NetworkManager.removeUserItemsEndPoint;
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(NetworkManager.apiUrl + apiEndPoint, NetworkManager.Instance.publicKey), items))
        {
            //TODO: remove this hack and use post before
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
                //exit the game
                Application.Quit();
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                Loader.instance.SetLoading(false);
                // the items sold count for the quests
                QuestManager.Instance.Refresh();
            }
        }
    }

    public int CurrentMoney
    {
        get => currentMoney;
        set
        {
            currentMoney = value;
            UpdateMoneyUI();
        }
    }
    public int CurrentGems
    {
        get => currentGems;
        set
        {
            currentGems = value;
            UpdateGemsUI();
        }
    }
    public List<Item> Items => items;
}