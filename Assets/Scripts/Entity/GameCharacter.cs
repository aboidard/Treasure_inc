using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A character of the user: who it is (the engine defines the characters, see its CharacterCatalog), what it
/// wears, by slot number, and the expedition it is leading, if any.
/// </summary>
public class GameCharacter
{
    /// <summary>Number of the character in the catalog of the server, from 1.</summary>
    public int num;
    public string name;
    /// <summary>"D" to "SSS", see CharacterRank.</summary>
    public string rank;
    /// <summary>How many times the user obtained the character: more than 1 after a duplicate was drawn.</summary>
    public int copies = 1;
    /// <summary>The stats of the character itself, with the keys of the stats of an item ("for", "con", "luk").</summary>
    public Dictionary<string, object> stats;
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

/// <summary>
/// The ranks of the characters, from the weakest to the strongest. The tier of a rank is its place in this
/// order (0 for D, 6 for SSS): the higher it is, the more impressive the summoning of the character.
/// </summary>
public static class CharacterRank
{
    private static readonly string[] ranks = { "D", "C", "B", "A", "S", "SS", "SSS" };

    private static readonly Color[] colors =
    {
        new Color(0.66f, 0.68f, 0.72f), // D: grey
        new Color(0.38f, 0.85f, 0.45f), // C: green
        new Color(0.32f, 0.62f, 1f),    // B: blue
        new Color(0.74f, 0.45f, 1f),    // A: purple
        new Color(1f, 0.82f, 0.25f),    // S: gold
        new Color(1f, 0.42f, 0.2f),     // SS: fire
        new Color(1f, 0.3f, 0.75f),     // SSS: shown as a rainbow where it can move, this color elsewhere
    };

    public const int TIER_MAX = 6;

    /// <summary>0 for D (and for an unknown rank) to 6 for SSS.</summary>
    public static int Tier(string rank)
    {
        int tier = System.Array.IndexOf(ranks, rank);
        return tier < 0 ? 0 : tier;
    }

    public static Color Color(string rank)
    {
        return Color(Tier(rank));
    }

    public static Color Color(int tier)
    {
        return colors[Mathf.Clamp(tier, 0, TIER_MAX)];
    }

    /// <summary>The color of a rank at a moment: the SSS cycles through the rainbow.</summary>
    public static Color AnimatedColor(int tier, float time)
    {
        return tier >= TIER_MAX ? UnityEngine.Color.HSVToRGB(Mathf.Repeat(time * 0.6f, 1f), 0.65f, 1f) : Color(tier);
    }

    /// <summary>The rank as a rich text in its color, e.g. for a description.</summary>
    public static string Colored(string rank)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(Color(rank)) + ">" + rank + "</color>";
    }
}

/// <summary>A character as sent by the API: its equipment is the item by slot number.</summary>
[System.Serializable]
public class CharacterAPI
{
    public int num { get; set; }
    public string name { get; set; }
    public string rank { get; set; }
    public Dictionary<string, object> stats { get; set; }
    public int copies { get; set; }
    public string expeditionId { get; set; }
    public Dictionary<string, ItemAPI> equipment { get; set; }
}

/// <summary>A portal the characters are summoned from, as sent by the API.</summary>
[System.Serializable]
public class PortalAPI
{
    public string id { get; set; }
    public string name { get; set; }
    /// <summary>Price of one draw.</summary>
    public int cost { get; set; }
    /// <summary>How many draws the "several draws at once" request makes.</summary>
    public int multi { get; set; }
    /// <summary>The chance of each rank, from the strongest to the weakest.</summary>
    public List<RankRateAPI> rates { get; set; }
    /// <summary>Every character the portal can give.</summary>
    public List<CharacterAPI> characters { get; set; }
}

[System.Serializable]
public class RankRateAPI
{
    public string rank { get; set; }
    public double percent { get; set; }
}

[System.Serializable]
public class RecruitRequest
{
    public string portal { get; set; }
    public int count { get; set; }
}

/// <summary>The answer to a recruitment: the characters drawn, in order, and the state of the user afterwards.</summary>
[System.Serializable]
public class RecruitResponse
{
    public List<DrawAPI> draws { get; set; }
    public int money { get; set; }
    public List<CharacterAPI> characters { get; set; }
}

/// <summary>A character drawn: new for the user, or a duplicate of one it already had.</summary>
[System.Serializable]
public class DrawAPI
{
    public int num { get; set; }
    public string name { get; set; }
    public string rank { get; set; }
    public Dictionary<string, object> stats { get; set; }
    public bool isNew { get; set; }
    public int copies { get; set; }
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
