using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField]
    private int currentMoney = 0;
    [SerializeField]
    private List<Item> items;
    public Text moneyText;
    [SerializeField]
    public bool loadingItems;
    public string initialItemLoaded;
    public string itemToAdd;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance d'Inventory dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        loadingItems = true;

        //lancer une tache de récupération des items du user
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.GET_USER_ITEMS));

        StartCoroutine(WaitForInitialLoading());
    }

    private IEnumerator WaitForInitialLoading()
    {
        //synchronisation avec NetworkManager
        while (loadingItems)
        {
            Debug.Log("Wait... " + loadingItems);
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("OK... " + loadingItems + " items : " + initialItemLoaded);
        InitItemsFromJSON();
        UpdateMoneyUI();
    }

    private void Update()
    {
        if (itemToAdd != string.Empty)
        {
            AddItemsFromJSON(itemToAdd);
            itemToAdd = string.Empty;
        }
    }

    public void InitItemsFromJSON()
    {
        if (initialItemLoaded == string.Empty) return;
        List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(initialItemLoaded);

        if (itemsFromJSON == null) return;
        foreach (ItemAPI it in itemsFromJSON)
        {
            AddItem(Item.CreateItemAPI(it));
        }
    }

    void UpdateMoneyUI()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        moneyText.text = currentMoney.ToString("#,0", nfi);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
    }

    public void SubtractMoney(int amount)
    {
        currentMoney -= amount;
        UpdateMoneyUI();
    }

    public void AddItem(Item item)
    {
        this.items.Add(item);
    }


    public void AddItemsFromJSON(string itemsJson)
    {
        List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(itemsJson);
        foreach (ItemAPI it in itemsFromJSON)
        {
            Item myItem = Item.CreateItemAPI(it);
            AddItem(myItem);
        }
    }

    public void AddItemsAndPersist(List<Item> itemsToAdd)
    {
        List<ItemAPI> listItemApi = new List<ItemAPI>();
        foreach (Item item in itemsToAdd)
        {
            ItemAPI itemApi = new ItemAPI(item);
            listItemApi.Add(itemApi);
        }

        string itemsJson = JsonConvert.SerializeObject(listItemApi);
        Task task = Task.Run(async () =>
        {
            await NetworkManager.instance.AddUserItems(itemsJson);
        });
    }

    public void SellAllItems()
    {
        var cost = 0;
        var nb = 0;
        foreach (var item in items)
        {
            AddMoney(item.price);
            cost += item.price;
            nb++;
        }
        items.Clear();
        if (nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
        }
    }

    public int CurrentMoney
    {
        get => currentMoney;
        set
        {
            currentMoney = value;
        }
    }
    public List<Item> Items => items;
}