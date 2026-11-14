using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    public GameObject itemInfoPanel;
    public new Text name;
    public Text properties;
    public Text description;
    public Image image;
    public static ItemInfoPanel instance;

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

    public void ShowItemInfo(Item item)
    {
        name.text = item.GetColoredName();
        // the stats of an equipment are shown with the price, above the description
        properties.text = "Prix : " + item.price;
        if (item.IsEquipable)
        {
            properties.text += "\n<b>" + Equipment.SlotName(item.Slot) + "</b>\n" + Equipment.Describe(item.stats);
        }
        // several lines must still fit in the rectangle of the text
        properties.supportRichText = true;
        properties.resizeTextMaxSize = Mathf.Max(properties.fontSize, properties.resizeTextMaxSize);
        properties.resizeTextMinSize = 14;
        properties.resizeTextForBestFit = true;
        description.text = item.description;
        image.sprite = item.graphics;

        OpenPanel();
    }

    public void OpenPanel()
    {
        itemInfoPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        itemInfoPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        itemInfoPanel.SetActive(false);
    }
}
