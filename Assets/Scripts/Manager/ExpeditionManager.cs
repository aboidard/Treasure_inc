using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager instance;
    public List<Expedition> expeditionList;
    
    private List<Expedition> expeditionToRemove;
    
    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance d'ExpeditionManager dans la scène");
            return;
        }
        instance = this;
    }

    void Start()
    {
        expeditionList = new List<Expedition>();
        expeditionToRemove = new List<Expedition>();
    }

    private void Update()
    {
        UpdateExpeditions();
    }

    private void UpdateExpeditions()
    {
        //traitement des expeditions
        foreach (var expedition in expeditionList)
        {
            expedition.UpdateTimeElapsed(Time.fixedDeltaTime);
            if(expedition.TimeElapsedSinceLastEvent >= expedition.efficiency)
            {
                EventManager.instance.GenerateEvent(expedition);
                expedition.TimeElapsedSinceLastEvent = expedition.TimeElapsedSinceLastEvent - 1f;
            }
            if(expedition.TimeElapsed >= expedition.timeScheduled)
            {
                ExpeditionReturn(expedition);
                expeditionToRemove.Add(expedition);
            }
        }

        //suppression des expeditions terminées
        foreach (var expedition in expeditionToRemove)
        {
            expeditionList.Remove(expedition);
        }
        if(expeditionToRemove.Count > 0)
        {
            expeditionToRemove.Clear();
        }
    }

    public Expedition SendExpedition(Location location, int time)
    {
        int cost = ComputeCost(time, location);

        if(Inventory.instance.CurrentMoney < cost) throw new NotEnoughtMoneyException();

        Expedition expedition = new Expedition();
        expedition.name = StringGenerator.ExpeditionNameGenerator();
        expedition.timeScheduled = time;
        expedition.location = location;

        expeditionList.Add(expedition);
        Inventory.instance.SubtractMoney(cost);
        Debug.Log("Expedition send" + expedition);
        return expedition;
    }

    public void ExpeditionReturn(Expedition ex)
    {
        Debug.Log("Expedition returned : " + ex);
        Inventory.instance.AddItems(ex.items);
    }

    public int ComputeCost(int time, Location location)
    {
        int cost = time * 1000 * location.level;

        return cost;
    }
    public Item GenerateItem()
    {
        int randomInt = Random.Range(1, ItemsDatabase.instance.allItems.Length + 1);
        return ItemsDatabase.instance.allItems.Single(x=> x.id == randomInt);
    }
}
