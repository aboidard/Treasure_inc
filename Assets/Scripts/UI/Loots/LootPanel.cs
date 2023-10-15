using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LootPanel : MonoBehaviour
{
    public GameObject lootPanel;
    public Text lootTitle;
    public Transform listObject;
    public GameObject objectPanelPrefab;
    public static LootPanel instance;
    private List<Item> items;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        ClosePanel();
    }

    public void ShowLoot(List<Item> items, string title)
    {
        lootTitle.text = title;
        OpenPanel();
        StartCoroutine(AddItem(items.Count));
    }

    IEnumerator AddItem(int nb)
    {
        Loader.instance.SetLoading(true);
        var apiEndPoint = NetworkManager.addUserItemsEndPoint;
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(NetworkManager.apiUrl + apiEndPoint, NetworkManager.Instance.publicKey), "{\"nb\":" + nb + "}"))
        {
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            Debug.Log("SendWebRequest : " + webRequest);
            yield return webRequest.SendWebRequest();
            try
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("ConnectionException: " + webRequest.error);
                    throw new ConnectionException(webRequest.error);
                }

                Debug.Log("Received: " + webRequest.downloadHandler.text);
                List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(webRequest.downloadHandler.text);
                this.items = new List<Item>();
                foreach (ItemAPI it in itemsFromJSON)
                {
                    Item myItem = Item.CreateItemAPI(it);
                    this.items.Add(myItem);

                    //add the item to the inventory
                    Inventory.Instance.AddItem(myItem);
                }
                InitPanel();
            }
            catch (ConnectionException e)
            {
                Debug.Log("Error: " + e.Message);
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de se connecter, veuillez réessayer ultérieurement !\n" + e.Message);
            }
            finally
            {
                Loader.instance.SetLoading(false);
            }
        }
    }

    public void OpenPanel()
    {
        lootPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        lootPanel.SetActive(true);
    }

    public void InitPanel()
    {
        for (int i = 0; i < listObject.childCount; i++)
        {
            Destroy(listObject.GetChild(i).gameObject);
        }

        for (int i = 0; i < this.items.Count; i++)
        {
            GameObject panel = Instantiate(objectPanelPrefab, listObject);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            if (objectPanel == null)
            {
                Debug.LogError("Object Panel prefab is missing ObjectPanel script.");
                return;
            }
            objectPanel.itemName.text = this.items[i].name;
            objectPanel.itemImage.sprite = this.items[i].graphics;
            objectPanel.itemBorder.color = Item.getRarityColor(this.items[i].rarity);

            objectPanel.item = this.items[i];
        }
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
}
