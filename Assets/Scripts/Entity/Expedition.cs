using System.Collections.Generic;
using UnityEngine;

public class Expedition
{
    public string expeditionName;
    public Location location;
    public List<CrewMember> crewMembers = new List<CrewMember>();
    public List<Item> items = new List<Item>();
    public int nbTtotalFloor;
    public int currentFloor = 1;
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
        return "\"" + expeditionName + "\" (" + location.name + ") : " + string.Join("\n", items);
    }

    public bool IsOver()
    {
        return panelRun.GetComponent<ExpeditionRunPanel>().endMiniGame;
    }
}

[System.Serializable]
public class ExpeditionAPI
{
    public int cost { get; set; }

    public ExpeditionAPI(int cost)
    {
        this.cost = cost;
    }
}