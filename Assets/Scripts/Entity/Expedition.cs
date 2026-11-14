using System.Collections.Generic;
using UnityEngine;

public class Expedition
{
    public string expeditionId;
    public string expeditionName;
    public Location location;
    public List<CrewMember> crewMembers = new List<CrewMember>();
    public int itemCount;
    public int nbTtotalFloor;
    public int currentFloor = 1;
    public int floorsCompleted = 0;
    public GameObject panelRun;
    public GameObject positionInGrid;
    public bool over = false;
    public bool floorOver = false;

    public void Init()
    {
        panelRun.GetComponent<ExpeditionRunPanel>().updateUI();
    }

    public override string ToString()
    {
        return "\"" + expeditionName + "\" (" + location.name + ") : " + itemCount + " objets";
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
}

[System.Serializable]
public class SendExpeditionResponse
{
    public string expeditionId { get; set; }
    public int money { get; set; }
    public int itemCount { get; set; }
}

[System.Serializable]
public class OpenChestRequest
{
    public string expeditionId { get; set; }
}