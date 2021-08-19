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
    public bool loadingMoney;

    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance d'Inventory dans la scène");
            return;
        }
        instance = this;
    }

    private void Start() 
    {
        loadingItems = true;
        loadingMoney = true;
        
        
        // items = new List<Item>();
        // //debug
        // for (int i = 0; i < 30; i++)
        // {
        //     AddItem(Item.GenerateRandomItem());
        // }

        UpdateMoneyUI();
    }

    public void InitItemsFromJSON(string itemsJson){
        Debug.Log(itemsJson);

        List<ItemFromAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemFromAPI>>(itemsJson);
        foreach (ItemFromAPI it in itemsFromJSON)
        {
            AddItem(Item.CreateItemFromAPI(it));
        }

    }

    void UpdateMoneyUI()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        moneyText.text = currentMoney.ToString("#,0.00", nfi);
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
        List<ItemFromAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemFromAPI>>(itemsJson);
        foreach (ItemFromAPI it in itemsFromJSON)
        {
            AddItem(Item.CreateItemFromAPI(it));
        }
    }

    public void AddItemsAndPersist(List<Item> itemsToAdd)
    {
        List<ItemFromAPI> listItemFromApi = new List<ItemFromAPI>();
        foreach (Item item in itemsToAdd) {
            ItemFromAPI itemApi = new ItemFromAPI(item);
            listItemFromApi.Add(itemApi);
        }

        string itemsJson = JsonConvert.SerializeObject(listItemFromApi);
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
            nb ++;
        }
        items.Clear();
        if(nb > 0)
        {
            MessageManager.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost);
        }
    }

    public float CurrentMoney => currentMoney;
    public List<Item> Items => items;
}