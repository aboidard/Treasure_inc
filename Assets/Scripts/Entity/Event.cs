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
