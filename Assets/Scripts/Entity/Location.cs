using UnityEngine;

[CreateAssetMenu(fileName = "Location", menuName = "ScriptableObject/Location")]
public class Location : ScriptableObject
{
    public int id;
    public new string name;
    public Sprite graphics;
    public Item[] lootTable;
    public int level = 1;

    public Location(int id, string name)
    {
        this.id = id;
        this.name = name;
    }
}
public enum EventLocationType
{
    None = 0,
    UpToOneBonus = 1,
    UpToTwoBonus = 2,
    UpToThreeBonus = 3,
    UpTofourBonus = 4
}
