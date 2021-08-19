using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if(instance != null){
            Debug.LogWarning("plus d'une instance de LootPanel dans la scène");
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
        this.items = items;
        OpenPanel();
        lootTitle.text = title;

        //init
        for (int i = 0; i < listObject.childCount; i++)
        {
            Destroy(listObject.GetChild(i).gameObject);
        }

        for (int i = 0; i < this.items.Count; i++)
        {
            GameObject panel = Instantiate(objectPanelPrefab, listObject);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            objectPanel.itemName.text = this.items[i].name;
            objectPanel.itemImage.sprite = this.items[i].graphics;
            objectPanel.itemBorder.color = Item.getRarityColor(this.items[i].rarity);

            objectPanel.item = this.items[i];

            //panel.GetComponents<Button>()[0].onClick.AddListener(delegate{objectPanel.GetItem();});
            //panel.GetComponents<Button>()[1].onClick.AddListener(delegate{objectPanel.SellItem();});
        }
    }

    public void SellAll()
    {
        var cost = 0;
        var nb = 0;
        for (int i = 0; i < this.items.Count; i++)
        {            
            Inventory.instance.AddMoney(this.items[i].price);
            cost += this.items[i].price;
            nb ++;
        }
        this.items.Clear();
        this.ClosePanel();
        if(nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
        }
    }

    public void TakeAll()
    {
        Inventory.instance.AddItemsAndPersist(this.items);
        this.items.Clear();
        this.ClosePanel();
    }

    public void OpenPanel()
    {
        
        lootPanel.transform.position =  new Vector3(Screen.width/2, Screen.height/2, 0);
        lootPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
}
