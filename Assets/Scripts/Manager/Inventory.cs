using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField]
    private int currentMoney = 100000;
    [SerializeField]
    private List<Item> items;
    public Text MoneyText;

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
        items = new List<Item>();
        //debug
        for (int i = 0; i < 30; i++)
        {
            AddItem(Item.GenerateRandomItem());
        }

        UpdateMoneyUI();
    }

    void UpdateMoneyUI()
    {
        var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = " ";
        MoneyText.text = currentMoney.ToString("#,0.00", nfi);
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
    
    public void AddItems(List<Item> items)
    {
        this.items.AddRange(items);
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
