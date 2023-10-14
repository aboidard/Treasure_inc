using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
[System.Serializable]
public class Item : ScriptableObject
{
    public int id;
    public new string name;
    public int price;
    public string description;
    public Rarity rarity = Rarity.Common;
    public Sprite graphics;
    public int graphicsId;

    public override string ToString()
    {
        string result = "id : " + this.id + " | name : " + this.name + " | rarity : ";
        result += "<color=" + getRarityColorString(this.rarity) + ">" + this.rarity + "</color> | price : " + this.price;

        return result;
    }

    public string GetColoredName()
    {
        return "<color=" + getRarityColorString(this.rarity) + ">" + this.name + "</color>";
    }

    /*public static List<Item> GenerateRandomItems(int nb)
    {
        List<Item> list = new List<Item>();
        for (int i = 0; i < nb; i++)
        {
            list.Add(GenerateRandomItem());
        }
        return list;
    }*/

    public bool Equals(Item other)
    {
        if (other == null) return false;
        return (this.id.Equals(other.id));
    }

    /* public static Item GenerateRandomItem()
     {
         int proba = Random.Range(1, 1001);
         Rarity rarity = Rarity.Common;
         switch (proba)
         {
             case int n when (n <= 650):
                 rarity = Rarity.Common;
                 break;
             case int n when (n > 650 && n <= 850):
                 rarity = Rarity.Uncommon;
                 break;
             case int n when (n > 850 && n <= 990):
                 rarity = Rarity.Rare;
                 break;
             case int n when (n > 990 && n <= 999):
                 rarity = Rarity.Epic;
                 break;
             case int n when (n == 1000):
                 rarity = Rarity.Legendary;
                 break;
         }
         return GenerateRandomItem(rarity);
     }
     public static Item GenerateRandomItem(Rarity rarity)
     {
         Item item = ScriptableObject.CreateInstance("Item") as Item;
         item.name = StringGenerator.ItemNameGenerator(rarity);
         item.price = Random.Range(1, 1000) + 1000 * (int)rarity;
         item.rarity = rarity;
         item.description = StringGenerator.ItemDescriptionGenerator();
         (item.graphics, item.graphicsId) = ItemManager.instance.PickOneRandomSprite();

         return item;
     }*/
    public static Item GenerateScriptableItem(int id)
    {
        // Item item = ScriptableObject.CreateInstance("Item") as Item;
        // AssetDatabase.CreateAsset(item, "Assets/Scripts/ScriptableObject/Items/pizza4fromages.asset");
        // AssetDatabase.SaveAssets();
        // AssetDatabase.Refresh();

        Item currentItem = ItemsDatabase.instance.allItems.Single(x => x.id == id);
        Inventory.Instance.AddItem(currentItem);

        Debug.Log("création d'un objet : " + currentItem);
        return currentItem;

    }

    public static Item CreateItemAPI(ItemAPI itemAPI)
    {
        Item item = ScriptableObject.CreateInstance("Item") as Item;
        item.id = itemAPI.id;
        item.name = itemAPI.name;
        item.price = itemAPI.price;
        item.rarity = (Rarity)System.Enum.Parse(typeof(Rarity), itemAPI.rarity, true);
        item.description = itemAPI.description;
        item.graphics = ItemManager.instance.PickSprite(itemAPI.graphics);
        item.graphicsId = itemAPI.graphics;
        return item;
    }

    public static string getRarityColorString(Rarity rarity)
    {
        string rarityColor;
        switch (rarity)
        {
            case Rarity.Uncommon:
                rarityColor = "green";
                break;
            case Rarity.Rare:
                rarityColor = "blue";
                break;
            case Rarity.Epic:
                rarityColor = "purple";
                break;
            case Rarity.Legendary:
                rarityColor = "orange";
                break;
            default:
                rarityColor = "white";
                break;
        }
        return rarityColor;
    }
    public static Color getRarityColor(Rarity rarity)
    {
        Color rarityColor;
        switch (rarity)
        {
            case Rarity.Uncommon:
                rarityColor = Color.green;
                break;
            case Rarity.Rare:
                rarityColor = Color.blue;
                break;
            case Rarity.Epic:
                rarityColor = Color.magenta;
                break;
            case Rarity.Legendary:
                rarityColor = Color.red;
                break;
            default:
                rarityColor = Color.white;
                break;
        }
        return rarityColor;
    }
}
public enum Rarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 5,
    Epic = 20,
    Legendary = 100
}

[System.Serializable]
public class ItemAPI
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string rarity { get; set; }
    public int graphics { get; set; }
    public int price { get; set; }

    public ItemAPI()
    {

    }

    public ItemAPI(Item item)
    {
        this.id = item.id;
        this.name = item.name;
        this.description = item.description;
        this.rarity = item.rarity.ToString();
        this.graphics = item.graphicsId;
        this.price = item.price;
    }
}