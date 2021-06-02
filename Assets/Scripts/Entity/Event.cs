using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "ScriptableObject/Event")]
public class Event : ScriptableObject
{
    public int id;
    public string text;
    public List<Item> lootTable;
    public float proba;
}

public enum EventLocationType
{
    None = 0,
    UpToOneBonus = 1,
    UpToTwoBonus = 2,
    UpToThreeBonus = 3,
    UpTofourBonus = 4
}
