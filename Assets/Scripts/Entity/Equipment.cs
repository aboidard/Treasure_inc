using System.Collections.Generic;
using System.Linq;

/// <summary>
/// What the client knows about the equipment: the slots of a character and the texts shown for the stats of an
/// item. The stats come from the server as one JSON object, e.g. {"slot": 3, "for": 5, "luk": 2, "spe1": "breakable"}:
/// the slot numbers and the skill ids are those of the engine (see its README), never change them here alone.
/// </summary>
public static class Equipment
{
    public const string SLOT = "slot";
    public const string SKILL_PREFIX = "spe";

    public const int SLOT_HEAD = 1;
    public const int SLOT_ARMOR = 2;
    public const int SLOT_WEAPON = 3;
    public const int SLOT_SHIELD = 4;
    public const int SLOT_BOOTS = 5;
    public const int SLOT_GLOVES = 6;
    public const int SLOT_RING = 7;
    public const int SLOT_AMULET = 8;
    public const int SLOT_TALISMAN = 9;
    public const int SLOT_COUNT = 9;

    private static readonly Dictionary<int, string> slotNames = new Dictionary<int, string>
    {
        { SLOT_HEAD, "Tête" },
        { SLOT_ARMOR, "Armure" },
        { SLOT_WEAPON, "Arme" },
        { SLOT_SHIELD, "Bouclier" },
        { SLOT_BOOTS, "Bottes" },
        { SLOT_GLOVES, "Gants" },
        { SLOT_RING, "Anneau" },
        { SLOT_AMULET, "Amulette" },
        { SLOT_TALISMAN, "Talisman" },
    };

    // the stats added to a character, in the order they are shown
    private static readonly KeyValuePair<string, string>[] statNames =
    {
        new KeyValuePair<string, string>("for", "Force"),
        new KeyValuePair<string, string>("con", "Constitution"),
        new KeyValuePair<string, string>("luk", "Chance"),
    };

    // the engine only sends the id of a skill: its description is local to the client
    private static readonly Dictionary<string, string> skillDescriptions = new Dictionary<string, string>
    {
        { "breakable", "Fragile : cet objet peut casser" },
        { "luck_charm", "Porte-bonheur : 25% de chance de rapporter un objet de plus à la fin de l'expédition" },
    };

    public static string SlotName(int slot)
    {
        return slotNames.TryGetValue(slot, out string name) ? name : "Emplacement " + slot;
    }

    /// <summary>The slot number of the stats of an item; 0 when the item cannot be equipped.</summary>
    public static int Slot(Dictionary<string, object> stats)
    {
        return Number(stats, SLOT);
    }

    /// <summary>The ids of the skills ("spe1", "spe2"...) of the stats of an item.</summary>
    public static List<string> Skills(Dictionary<string, object> stats)
    {
        if (stats == null) return new List<string>();
        return stats.Where(stat => stat.Key.StartsWith(SKILL_PREFIX) && stat.Value != null)
            .OrderBy(stat => stat.Key)
            .Select(stat => stat.Value.ToString())
            .ToList();
    }

    public static string SkillDescription(string skill)
    {
        return skillDescriptions.TryGetValue(skill, out string description) ? description : skill;
    }

    /// <summary>One line per stat ("Force +5") then one per skill; empty when the item cannot be equipped.</summary>
    public static string Describe(Dictionary<string, object> stats)
    {
        if (stats == null) return "";
        List<string> lines = new List<string>();
        foreach (var stat in statNames)
        {
            int value = Number(stats, stat.Key);
            if (value != 0) lines.Add(stat.Value + " " + Signed(value));
        }
        foreach (string skill in Skills(stats))
        {
            lines.Add("<i>" + SkillDescription(skill) + "</i>");
        }
        return string.Join("\n", lines);
    }

    /// <summary>The stats on one short line, e.g. "FOR +5 CHA +2".</summary>
    public static string Summary(Dictionary<string, object> stats)
    {
        if (stats == null) return "";
        List<string> parts = new List<string>();
        foreach (var stat in statNames)
        {
            int value = Number(stats, stat.Key);
            if (value != 0) parts.Add(stat.Value.Substring(0, 3).ToUpper() + " " + Signed(value));
        }
        if (Skills(stats).Count > 0) parts.Add("★");
        return string.Join(" ", parts);
    }

    /// <summary>
    /// The sum of each stat over several items, as lines ("Force +12"), then every skill once.
    /// baseStats are the stats of the character wearing the items, added to the sums; null for none.
    /// </summary>
    public static string DescribeTotal(IEnumerable<Item> items, Dictionary<string, object> baseStats = null)
    {
        Dictionary<string, object> total = new Dictionary<string, object>();
        foreach (var stat in statNames)
        {
            total[stat.Key] = Number(baseStats, stat.Key);
        }
        HashSet<string> skills = new HashSet<string>();
        foreach (Item item in items)
        {
            foreach (var stat in statNames)
            {
                total[stat.Key] = Number(total, stat.Key) + Number(item.stats, stat.Key);
            }
            skills.UnionWith(Skills(item.stats));
        }
        List<string> lines = new List<string>();
        foreach (var stat in statNames)
        {
            lines.Add(stat.Value + " " + Signed(Number(total, stat.Key)));
        }
        foreach (string skill in skills.OrderBy(s => s))
        {
            lines.Add("<i>" + SkillDescription(skill) + "</i>");
        }
        return string.Join("\n", lines);
    }

    public enum ChangeKind { StatUp, StatDown, SkillGained, SkillLost }

    /// <summary>One difference between two items of the same slot, e.g. StatUp "Force +3".</summary>
    public struct Change
    {
        public ChangeKind kind;
        public string text;

        public bool IsGood => kind == ChangeKind.StatUp || kind == ChangeKind.SkillGained;
        public bool IsSkill => kind == ChangeKind.SkillGained || kind == ChangeKind.SkillLost;
    }

    /// <summary>
    /// What changes for a character when the stats "current" are replaced by "next": the stats that differ, then
    /// the skills gained, then those lost. Either can be null (empty slot).
    /// </summary>
    public static List<Change> Compare(Dictionary<string, object> current, Dictionary<string, object> next)
    {
        List<Change> changes = new List<Change>();
        foreach (var stat in statNames)
        {
            int delta = Number(next, stat.Key) - Number(current, stat.Key);
            if (delta == 0) continue;
            changes.Add(new Change
            {
                kind = delta > 0 ? ChangeKind.StatUp : ChangeKind.StatDown,
                text = stat.Value + " " + Signed(delta),
            });
        }
        List<string> before = Skills(current);
        List<string> after = Skills(next);
        foreach (string skill in after.Except(before))
        {
            changes.Add(new Change { kind = ChangeKind.SkillGained, text = SkillDescription(skill) });
        }
        foreach (string skill in before.Except(after))
        {
            changes.Add(new Change { kind = ChangeKind.SkillLost, text = SkillDescription(skill) });
        }
        return changes;
    }

    // Json.NET reads the numbers of an untyped object as long
    private static int Number(Dictionary<string, object> stats, string key)
    {
        if (stats == null || !stats.TryGetValue(key, out object value) || value == null) return 0;
        try
        {
            return System.Convert.ToInt32(value);
        }
        catch (System.Exception)
        {
            return 0;
        }
    }

    private static string Signed(int value)
    {
        return (value >= 0 ? "+" : "") + value;
    }
}
