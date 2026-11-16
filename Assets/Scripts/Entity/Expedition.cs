using System.Collections.Generic;
using UnityEngine;

public class Expedition
{
    public string expeditionId;
    public string expeditionName;
    public Location location;
    public List<CrewMember> crewMembers = new List<CrewMember>();
    /// <summary>The character leading the expedition: the one shown in the mini game, busy until the chest is opened.</summary>
    public GameCharacter character;
    public int itemCount;
    public int nbTtotalFloor;
    public int currentFloor = 1;
    public int floorsCompleted = 0;
    public GameObject panelRun;
    public GameObject positionInGrid;
    public bool over = false;
    public bool floorOver = false;
    /// <summary>
    /// What happens to the character on the way, as decided by the server when the expedition was sent: the
    /// obstacles of each floor and its fights. Null for an expedition recovered after a restart.
    /// </summary>
    public ExpeditionRunAPI run;
    /// <summary>The life of the character, kept from a floor to the next.</summary>
    public int hp;
    public int maxHp;
    /// <summary>True once the character died: the expedition comes back at once, with what it found before.</summary>
    public bool failed = false;

    /// <summary>What the server planned for the current floor; null when it planned nothing.</summary>
    public ExpeditionFloorAPI CurrentFloor
    {
        get
        {
            if (run == null || run.floors == null || currentFloor < 1 || currentFloor > run.floors.Count) return null;
            return run.floors[currentFloor - 1];
        }
    }

    public void Init()
    {
        panelRun.GetComponent<ExpeditionRunPanel>().updateUI();
    }

    public override string ToString()
    {
        return "\"" + expeditionName + "\" (" + (location != null ? location.name : "?") + ") : " + itemCount + " objets";
    }

    public bool IsOver()
    {
        return panelRun.GetComponent<ExpeditionRunPanel>().endMiniGame;
    }

    /// <summary>The loot found so far: it grows with the floors, up to the items of the chest on the last floor.</summary>
    public int LootFound()
    {
        return itemCount * floorsCompleted / nbTtotalFloor;
    }
}

[System.Serializable]
public class DigCaseRequest
{
    public string location { get; set; }
    public int caseId { get; set; }
    public int cost { get; set; }
}

[System.Serializable]
public class DigCaseResponse
{
    public int rarity { get; set; }
    public int money { get; set; }
}

[System.Serializable]
public class SendExpeditionRequest
{
    public string location { get; set; }
    public int difficulty { get; set; }
    public int? caseId { get; set; }
    /// <summary>Number of the character leading the expedition.</summary>
    public int character { get; set; }
}

[System.Serializable]
public class SendExpeditionResponse
{
    public string expeditionId { get; set; }
    public int money { get; set; }
    public int itemCount { get; set; }
    /// <summary>The experience the character will earn when the chest is opened.</summary>
    public int xp { get; set; }
    /// <summary>What happens to the character on the way; null from a server older than the fights.</summary>
    public ExpeditionRunAPI run { get; set; }
}

/// <summary>
/// The way of a character through its expedition, as sent by the API: the client only shows it. The floors stop
/// where the character died, when it did.
/// </summary>
[System.Serializable]
public class ExpeditionRunAPI
{
    public int maxHp { get; set; }
    public int floorsCompleted { get; set; }
    public bool dead { get; set; }
    public List<ExpeditionFloorAPI> floors { get; set; }
}

[System.Serializable]
public class ExpeditionFloorAPI
{
    /// <summary>What stands in the way, in the order it is met.</summary>
    public List<EncounterAPI> encounters { get; set; }
    /// <summary>The life recovered at the end of the floor.</summary>
    public int healed { get; set; }
    /// <summary>The life of the character when it leaves the floor; 0 when it died there.</summary>
    public int hpEnd { get; set; }
}

/// <summary>A barricade, a monster or the guardian of the chest of a floor.</summary>
[System.Serializable]
public class EncounterAPI
{
    public const string BARRICADE = "barricade";
    public const string MONSTER = "monster";
    public const string GUARDIAN = "guardian";

    public string type { get; set; }
    /// <summary>Where it stands, from the start of the floor (0) to its chest (100).</summary>
    public int position { get; set; }
    /// <summary>The solidity of a barricade, the life of a monster.</summary>
    public int hp { get; set; }
    /// <summary>How long the character is stopped, in milliseconds.</summary>
    public int duration { get; set; }
    /// <summary>Null for a barricade.</summary>
    public MonsterAPI monster { get; set; }
    /// <summary>Null for a barricade.</summary>
    public FightAPI fight { get; set; }

    public bool IsBarricade => type == BARRICADE;
    public bool IsGuardian => type == GUARDIAN;
}

[System.Serializable]
public class MonsterAPI
{
    /// <summary>Which monster it is: see MonsterArt for its look.</summary>
    public string kind { get; set; }
    public string name { get; set; }
    public int hp { get; set; }
    public int attack { get; set; }
    public int defense { get; set; }
    public int interval { get; set; }
}

/// <summary>A fight, already played by the server: its events are shown at their time.</summary>
[System.Serializable]
public class FightAPI
{
    public bool win { get; set; }
    public int duration { get; set; }
    public int hpStart { get; set; }
    public int hpEnd { get; set; }
    public List<FightEventAPI> events { get; set; }
}

[System.Serializable]
public class FightEventAPI
{
    public const string HIT = "hit";
    public const string HEAL = "heal";
    public const string DODGE = "dodge";
    public const string SKILL = "skill";
    public const string CHARACTER = "c";

    /// <summary>Milliseconds since the fight started.</summary>
    public int t { get; set; }
    public string type { get; set; }
    /// <summary>"c" for the character, "m" for the monster.</summary>
    public string by { get; set; }
    /// <summary>The id of the skill of the character it comes from; null for a plain hit.</summary>
    public string skill { get; set; }
    public int amount { get; set; }
    public bool crit { get; set; }
    /// <summary>The life of the character afterwards.</summary>
    public int hp { get; set; }
    public int monsterHp { get; set; }

    public bool ByCharacter => by == CHARACTER;
}

[System.Serializable]
public class OpenChestRequest
{
    public string expeditionId { get; set; }
}

/// <summary>The content of a chest: its items, and what the expedition brought to its character (null: nobody led it).</summary>
[System.Serializable]
public class OpenChestResponse
{
    public List<ItemAPI> items { get; set; }
    public XpGainAPI xp { get; set; }
    /// <summary>How many floors the expedition had; 0 from a server older than the fights.</summary>
    public int floors { get; set; }
    /// <summary>How many of them the character finished: fewer than floors when it died on the way.</summary>
    public int floorsCompleted { get; set; }
}