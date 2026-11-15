using UnityEngine;
using UnityEngine.UI;

public class ObjectPanel : MonoBehaviour
{
    public Text itemName;
    public Image itemImage;
    public Image itemBorder;
    public Item item;
    public Toggle deleteToggle;
    private GameObject gearIcon;
    private static Sprite gearSprite;

    /// <summary>Shows an item: its name, its picture, the color of its rarity and a gear when it can be equipped.</summary>
    public void SetItem(Item item)
    {
        this.item = item;
        itemName.text = item.name;
        itemImage.sprite = item.graphics;
        itemBorder.color = Item.getRarityColor(item.rarity);
        ShowGear(item.IsEquipable);
    }

    private void ShowGear(bool equipable)
    {
        if (gearIcon == null)
        {
            if (!equipable) return;
            if (gearSprite == null) gearSprite = Resources.Load<Sprite>("Sprites/gear");
            // small, in the bottom right corner of the picture; it lets the clicks reach the buttons
            RectTransform rect = UIBuilder.Child("Gear", itemImage.transform, new Vector2(0.72f, 0.02f), new Vector2(0.98f, 0.28f));
            UIBuilder.Image(rect, gearSprite, Color.white, false).preserveAspect = true;
            gearIcon = rect.gameObject;
        }
        gearIcon.SetActive(equipable);
    }

    public void GetItem()
    {
        // Inventory inventory = Inventory.Instance;
        // if(inventory.coinsCount >= item.price)
        // {
        //     inventory.content.Add(item);
        //     inventory.UpdateInventoryUI();
        //     inventory.coinsCount -= item.price;
        //     inventory.UpdateTextUI();
        // }
    }
    public void SellItem()
    {
        // Inventory inventory = Inventory.Instance;
        // if(inventory.coinsCount >= item.price)
        // {
        //     inventory.content.Add(item);
        //     inventory.UpdateInventoryUI();
        //     inventory.coinsCount -= item.price;
        //     inventory.UpdateTextUI();
        // }
    }

    public void GetUiItem()
    {

    }

    public void OnClickInfoButton()
    {
        // while a slot is being filled, a click on an item equips it
        if (InventoryPanel.instance != null && InventoryPanel.instance.IsEquipMode && InventoryPanel.instance.OwnsPanel(this))
        {
            InventoryPanel.instance.EquipItem(item);
            return;
        }
        ItemInfoPanel.instance.ShowItemInfo(item);
    }
}
