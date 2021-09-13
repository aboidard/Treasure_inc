using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : Panel
{
    public Transform listObject;
    public GameObject objectPanelPrefab;
    public static InventoryPanel instance;

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
    }

    public void SellAll()
    {
        var cost = 0;
        var nb = 0;
        for (int i = 0; i < Inventory.instance.Items.Count; i++)
        {
            Inventory.instance.AddMoney(Inventory.instance.Items[i].price);
            cost += Inventory.instance.Items[i].price;
            nb++;
        }
        Inventory.instance.Items.Clear();
        this.Close();
        if (nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
        }
    }
}
