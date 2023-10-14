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
            moneyText = GameObject.FindGameObjectsWithTag("MoneyText")[0].GetComponent<Text>();
        }
        moneyText.text = currentMoney.ToString("#,0", nfi);
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
    public List<Item> Items => items;
}