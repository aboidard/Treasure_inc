using UnityEngine;
using UnityEngine.UI;

public class ObjectPanel : MonoBehaviour
{
    public Text itemName;
    public Image itemImage;
    public Image itemBorder;
    public Item item;
    public Toggle deleteToggle;

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
        ItemInfoPanel.instance.ShowItemInfo(item);
    }
}
