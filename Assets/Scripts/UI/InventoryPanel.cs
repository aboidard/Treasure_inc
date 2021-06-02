using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    public GameObject inventoryPanel;    
    public Transform listObject;
    public GameObject objectPanelPrefab;
    public static InventoryPanel instance;
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

    public void ShowInventory()
    {
        if(IsPanelOpen())
        {
            ClosePanel();
            return;
        }

        //init
        for (int i = 0; i < listObject.childCount; i++)
        {
            Destroy(listObject.GetChild(i).gameObject);
        }

        for (int i = 0; i < Inventory.instance.Items.Count; i++)
        {
            GameObject panel = Instantiate(objectPanelPrefab, listObject);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            objectPanel.itemName.text = Inventory.instance.Items[i].name;
            objectPanel.itemImage.sprite = Inventory.instance.Items[i].graphics;
            objectPanel.itemBorder.color = Item.getRarityColor(Inventory.instance.Items[i].rarity);

            objectPanel.item = Inventory.instance.Items[i];

            //panel.GetComponents<Button>()[0].onClick.AddListener(delegate{objectPanel.GetItem();});
            //panel.GetComponents<Button>()[1].onClick.AddListener(delegate{objectPanel.SellItem();});
        }
        
        OpenPanel();
    }

    public void SellAll()
    {
        var cost = 0;
        var nb = 0;
        for (int i = 0; i < Inventory.instance.Items.Count; i++)
        {            
            Inventory.instance.AddMoney(Inventory.instance.Items[i].price);
            cost += Inventory.instance.Items[i].price;
            nb ++;
        }
        Inventory.instance.Items.Clear();
        this.ClosePanel();
        if(nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
        }
    }

    public void OpenPanel()
    {
        
        inventoryPanel.transform.position =  new Vector3(Screen.width/2, Screen.height/2, 0);
        inventoryPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        inventoryPanel.SetActive(false);
    }
    private bool IsPanelOpen()
    {
        return inventoryPanel.activeInHierarchy;
    }
}
