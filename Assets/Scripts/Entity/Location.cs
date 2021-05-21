using UnityEngine;
using UnityEngine.UI;

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
