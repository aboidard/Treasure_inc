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

    public void ShowLoot(List<Item> items, string title)
    {
        int requestId = Inventory.Instance.AddItemsAndPersist(items);
        //start a coroutine to wait for the request to be done
        StartCoroutine(WaitForRequest(requestId, title));

    }
    public IEnumerator<WaitForSeconds> WaitForRequest(int requestId, string title)
    {
        NetworkResponseItems response = null;
        var start = Time.time;
        while (response == null || Time.time - start < 5)
        {
            //response = (NetworkResponseItems)NetworkManager.Instance.GetResponse(requestId);
            yield return new WaitForSeconds(0.1f);
        }

        OpenPanel();
        lootTitle.text = title;
        this.items = response.getItems();

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
        }
    }

    public void OpenPanel()
    {

        lootPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        lootPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
}
