using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager instance;
    public List<Expedition> expeditionList;
    public GameObject expeditionPanelPrefab;
    public GameObject expeditionsPanelGrid;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance d'ExpeditionManager dans la scène");
            return;
        }
        instance = this;
    }

    void Start()
    {
        expeditionList = new List<Expedition>();

        //init
        for (int i = 0; i < expeditionsPanelGrid.transform.childCount; i++)
        {
            Destroy(expeditionsPanelGrid.transform.GetChild(i).gameObject);
        }
    }

    private void FixedUpdate()
    {
        UpdateExpeditions();
    }

    private void UpdateExpeditions()
    {
        //traitement des expeditions
        foreach (var expedition in expeditionList)
        {
            //expedition.Update();
            if (expedition.floorOver)
            {
                expedition.currentFloor++;
                //calcul si l'expedition est finie
                if (expedition.currentFloor > expedition.nbTtotalFloor)
                {
                    expedition.over = true;
                    ExpeditionReturn(expedition);
                    expeditionList.Remove(expedition);
                    Destroy(expedition.panelRun);
                }
                else
                {
                    Destroy(expedition.panelRun);
                    expedition.panelRun = Instantiate(expeditionPanelPrefab, expeditionsPanelGrid.transform);
                    expedition.panelRun.GetComponent<ExpeditionRunPanel>().expedition = expedition;
                    expedition.floorOver = false;
                    expedition.UpdateTitle();
                }
            }
        }
    }

    public Expedition SendExpedition(Location location, int floor)
    {
        int cost = ComputeCost(floor, location);

        if (Inventory.instance.CurrentMoney < cost) throw new NotEnoughtMoneyException();

        Inventory.instance.SubtractMoney(cost);
        Expedition expedition = new Expedition();
        expedition.expeditionName = StringGenerator.ExpeditionNameGenerator();
        expedition.location = location;
        expedition.nbTtotalFloor = floor;
        expedition.panelRun = Instantiate(expeditionPanelPrefab, expeditionsPanelGrid.transform);
        expedition.panelRun.GetComponent<ExpeditionRunPanel>().expedition = expedition;
        expedition.Init();
        expeditionList.Add(expedition);

        Debug.Log("Expedition send " + expedition);
        return expedition;
    }

    public void ExpeditionReturn(Expedition ex)
    {
        Debug.Log("Expedition returned : " + ex);
        LootPanel.instance.ShowLoot(ex.items, "l'expedition \"" + ex.expeditionName + "\" est de retour !");
    }

    public int ComputeCost(int time, Location location)
    {
        int cost = time * 1000 * location.level;

        return cost;
    }
    public Item GenerateItem()
    {
        int randomInt = Random.Range(1, ItemsDatabase.instance.allItems.Length + 1);
        return ItemsDatabase.instance.allItems.Single(x => x.id == randomInt);
    }
}
