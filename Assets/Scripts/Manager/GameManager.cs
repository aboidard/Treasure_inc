using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{    
    public static GameManager instance;

    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de GameManager dans la scène");
            return;
        }
        instance = this;
    }

    public void Update()
    {
        Inputs();
    }

    private void Inputs()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ExpeditionManager.instance.ExpeditionReturn(ExpeditionManager.instance.expeditionList[0]);
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            Dictionary<Rarity, int> dictionary = new Dictionary<Rarity, int>();
            foreach (Rarity rarity in (Rarity[]) Enum.GetValues(typeof(Rarity)))
            {
                dictionary.Add(rarity, 0);
            }
            List<Item> items = new List<Item>();
            for (var i = 0; i < 1000; i++)
            {
                Item item = Item.GenerateRandomItem();
                int result;
                if(dictionary.TryGetValue(item.rarity, out result))
                {
                    dictionary[item.rarity] = result + 1;
                }
                items.Add(item);
            }
            Debug.Log(string.Join(Environment.NewLine, dictionary));
            Debug.Log(string.Join(Environment.NewLine, items));
        }
    }
}
