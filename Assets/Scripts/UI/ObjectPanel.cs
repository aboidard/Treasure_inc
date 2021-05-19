using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObjectPanel : MonoBehaviour, IPointerEnterHandler
{
    public Text itemName;
    public Image itemImage;
    public Image itemBorder;
    public Item item;
	
    public void GetItem()
    {
        // Inventory inventory = Inventory.instance;
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
        // Inventory inventory = Inventory.instance;
        // if(inventory.coinsCount >= item.price)
        // {
        //     inventory.content.Add(item);
        //     inventory.UpdateInventoryUI();
        //     inventory.coinsCount -= item.price;
        //     inventory.UpdateTextUI();
        // }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
         Debug.Log("au dessus !" + item );
    }

    public void OnClickInfoButton()
    {
        ItemInfoPanel.instance.ShowItemInfo(item);
    }
}
