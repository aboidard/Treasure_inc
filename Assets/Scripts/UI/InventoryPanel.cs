using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : Panel
{
    public GameObject listObject;
    private List<ObjectPanel> listObjectPanel;
    public GameObject objectPanelPrefab;
    public static InventoryPanel instance;
    public Text title;
    public bool sellMode = false;
    public GameObject sellModeButton;
    public GameObject sellYesButton;
    public GameObject sellNoButton;


    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }
    protected override void WillShow()
    {
        sellModeButton.SetActive(true);
        sellNoButton.SetActive(false);
        sellYesButton.SetActive(false);
        //init
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            Destroy(listObject.transform.GetChild(i).gameObject);
        }

        listObjectPanel = new List<ObjectPanel>();
        for (int i = 0; i < Inventory.instance.Items.Count; i++)
        {
            GameObject panel = Instantiate(objectPanelPrefab, listObject.transform);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            listObjectPanel.Add(objectPanel);
            objectPanel.itemName.text = Inventory.instance.Items[i].name;
            objectPanel.itemImage.sprite = Inventory.instance.Items[i].graphics;
            objectPanel.itemBorder.color = Item.getRarityColor(Inventory.instance.Items[i].rarity);

            objectPanel.item = Inventory.instance.Items[i];
        }
        float width = listObject.GetComponent<RectTransform>().rect.width;
        Vector2 newSize = new Vector2(width / 3, width / 3);
        listObject.GetComponent<GridLayoutGroup>().cellSize = newSize;
    }

    public void ToggleSellMode()
    {
        sellMode = !sellMode;
        this.title.text = sellMode ? "Vendre" : "Inventaire";
        sellModeButton.SetActive(!sellModeButton.activeInHierarchy);
        sellNoButton.SetActive(!sellModeButton.activeInHierarchy);
        sellYesButton.SetActive(!sellModeButton.activeInHierarchy);
        foreach (ObjectPanel item in listObjectPanel)
        {
            Toggle toggle = item.deleteToggle;
            toggle.gameObject.SetActive(!sellModeButton.activeInHierarchy);
        }
    }

    public void SellItems()
    {
        List<Item> listToSell = new List<Item>();
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            ObjectPanel objectPanel = listObject.transform.GetChild(i).GetComponent<ObjectPanel>();
            if (objectPanel.deleteToggle.isOn)
            {
                listToSell.Add(objectPanel.item);
            }
        }

        var cost = 0;
        var nb = 0;
        for (int i = 0; i < listToSell.Count; i++)
        {
            Inventory.instance.AddMoney(listToSell[i].price);
            cost += listToSell[i].price;
            Inventory.instance.Items.Remove(listToSell[i]);
            nb++;
        }
        if (nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
            ToggleSellMode();
            Close();
        }
    }
}
