using UnityEngine;

public class UIManager : MonoBehaviour
{
    public void ShowItemInfoPanel(Item item)
    {
        ItemInfoPanel.instance.ShowItemInfo(item);
    }
}
