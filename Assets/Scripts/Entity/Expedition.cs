using System.Collections.Generic;
using UnityEngine;

public class Expedition
{
    public string expeditionName;
    public float difficulty;
    public int distance;
    public Location location;
    public List<CrewMember> crewMembers;
    public List<Item> items;
    public int nbTtotalFloor;
    public int currentFloor;
    public GameObject panelRun;
    public bool over = false;
    public bool floorOver = false;

    public void Init()
    {
        this.items = new List<Item>();
        this.crewMembers = new List<CrewMember>();
        this.currentFloor = 1;
        UpdateTitle();
    }

    public override string ToString()
    {
        return "\"" + expeditionName + "\" (" + location.name + ") : " + string.Join("\n", items);
    }

    public void UpdateTitle()
    {
        panelRun.GetComponent<ExpeditionRunPanel>().title.text = this.expeditionName + " - étage " + this.currentFloor + "/" + this.nbTtotalFloor;
    }

    public bool IsOver()
    {
        return panelRun.GetComponent<ExpeditionRunPanel>().endMiniGame;
    }
}
