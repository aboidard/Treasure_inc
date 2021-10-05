using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager instance;
    public List<Expedition> expeditionList;
    public GameObject expeditionPanelPrefab;
    public GameObject expeditionsPanelGrid;
    public GameObject LootPanelGrid;
    public GameObject LootButtonPrefab;

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
        List<Expedition> expeditionToRemove = null;
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
                    if (expeditionToRemove == null)
                    {
                        expeditionToRemove = new List<Expedition>();
                    }
                    expeditionToRemove.Add(expedition);
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

        CleanExpeditions(expeditionToRemove);
    }

    private void CleanExpeditions(List<Expedition> listToRemove)
    {
        if (listToRemove != null)
        {
            foreach (Expedition expedition in listToRemove)
            {
                expeditionList.Remove(expedition);
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
        //expedition.panelRun.transform.SetParent(expeditionsPanelGrid.transform, true);
        expedition.panelRun.GetComponent<ExpeditionRunPanel>().expedition = expedition;
        expedition.Init();
        expeditionList.Add(expedition);

        string[] expeditionJson = { JsonConvert.SerializeObject(new ExpeditionAPI(cost)) };
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.SEND_EXPEDITION, expeditionJson));

        Debug.Log("Expedition send " + expedition);
        return expedition;
    }

    public void ExpeditionReturn(Expedition ex)
    {
        Debug.Log("Expedition returned : " + ex);
        GameObject lootButton = Instantiate(LootButtonPrefab, LootPanelGrid.transform);
        lootButton.GetComponent<LootButton>().ex = ex;
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
