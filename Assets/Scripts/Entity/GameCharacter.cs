using System.Collections.Generic;
using UnityEngine;

/// <summary>A character of the user: what it wears, by slot number, and the expedition it is leading, if any.</summary>
public class GameCharacter
{
    /// <summary>Number of the character for the server, from 1.</summary>
    public int num;
    public string name;
    public Sprite graphics;
    /// <summary>Null unless the character is on an expedition whose chest is not opened yet.</summary>
    public string expeditionId;
    public Dictionary<int, Item> equipment = new Dictionary<int, Item>();

    public bool IsAvailable => string.IsNullOrEmpty(expeditionId);

    public Item Equipped(int slot)
    {
        return equipment.TryGetValue(slot, out Item item) ? item : null;
    }
}

/// <summary>A character as sent by the API: its equipment is the item by slot number.</summary>
[System.Serializable]
public class CharacterAPI
{
    public int num { get; set; }
    public string expeditionId { get; set; }
    public Dictionary<string, ItemAPI> equipment { get; set; }
}

[System.Serializable]
public class EquipItemRequest
{
    public int slot { get; set; }
    public int itemId { get; set; }
}

[System.Serializable]
public class UnequipItemRequest
{
    public int slot { get; set; }
}
