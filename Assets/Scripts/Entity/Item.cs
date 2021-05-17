using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObject/Item")]
public class Item : ScriptableObject
{
    public int id;
    public new string name;
    public int price;

    public Rarity rarity = Rarity.Common;

    public override string ToString()
    {
        string result =  "id : " + this.id + " | name : " + this.name + " | rarity : " ;
        string rarityColor;
        switch (this.rarity)
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
        result += "<color=" + rarityColor + ">" + this.rarity + "</color> | price : " + this.price; 

        return result;
    }

    public static Item GenerateRandomItem()
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
        item.price = Random.Range(1, 1000) + 1000 * (int) rarity;
        item.rarity = rarity;
        int randomInt = Random.Range(1, ItemsDatabase.instance.allItems.Length + 1);
        return item;
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
