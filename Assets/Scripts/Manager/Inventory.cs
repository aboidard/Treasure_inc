using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField]
    private int currentMoney = 0;
    [SerializeField]
    private List<Item> items;
    public Text moneyText;

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
        //lancer une tache de récupération des items du user
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.GET_USER_ITEMS));
    }

    public void UpdateMoneyUI()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        if (moneyText == null)
        {
            moneyText = GameObject.FindGameObjectsWithTag("MoneyText")[0].GetComponent<Text>();
        }
        moneyText.text = currentMoney.ToString("#,0", nfi);
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (SceneManager.GetActiveScene().name == "MainScene") UpdateMoneyUI();
    }

    public void SubtractMoney(int amount)
    {
        currentMoney -= amount;
        if (SceneManager.GetActiveScene().name == "MainScene") UpdateMoneyUI();
    }

    public void AddItem(Item item)
    {
        this.items.Add(item);
    }

    public void AddItemsAndPersist(List<Item> itemsToAdd)
    {
        List<ItemAPI> listItemApi = new List<ItemAPI>();
        foreach (Item item in itemsToAdd)
        {
            ItemAPI itemApi = new ItemAPI(item);
            listItemApi.Add(itemApi);
        }

        string[] itemsJson = { JsonConvert.SerializeObject(listItemApi) };
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.ADD_USER_ITEMS, itemsJson));
    }

    public void RemoveItemsAndPersist(List<Item> itemsToRemove)
    {
        List<ItemAPI> listItemApi = new List<ItemAPI>();
        foreach (Item item in itemsToRemove)
        {
            ItemAPI itemApi = new ItemAPI(item);
            listItemApi.Add(itemApi);
            items.Remove(item);
        }

        string[] itemsJson = { JsonConvert.SerializeObject(listItemApi) };
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.REMOVE_USER_ITEMS, itemsJson));
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