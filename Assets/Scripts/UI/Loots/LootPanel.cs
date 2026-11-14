using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public void ShowLoot(string expeditionId, string title)
    {
        lootTitle.text = title;
        InitPanel();
        OpenPanel();
        StartCoroutine(OpenChest(expeditionId));
    }

    IEnumerator OpenChest(string expeditionId)
    {
        Loader.instance.SetLoading(true);
        long status = 0;
        string response = null;
        yield return NetworkManager.Instance.PostJson(NetworkManager.addUserItemsEndPoint,
            new OpenChestRequest { expeditionId = expeditionId }, (code, text) =>
            {
                status = code;
                response = text;
            });
        Loader.instance.SetLoading(false);

        if (status != 201)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", "Impossible d'ouvrir le coffre, veuillez réessayer ultérieurement !\n" + response);
            yield break;
        }

        List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(response);
        this.items = new List<Item>();
        foreach (ItemAPI it in itemsFromJSON)
        {
            Item myItem = Item.CreateItemAPI(it);
            this.items.Add(myItem);

            //add the item to the inventory
            Inventory.Instance.AddItem(myItem);
        }
        FillPanel();
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
    }
    public void FillPanel()
    {


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
