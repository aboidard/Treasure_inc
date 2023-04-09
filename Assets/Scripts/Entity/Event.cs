using System.Collections.Generic;
using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject eventObject;

    public virtual bool Reach()
    {
        return true;
    }

    public virtual List<Item> GetReward()
    {
        return null;
    }

    public virtual void SetRewards(List<Item> items)
    {
    }

    public virtual void Start()
    {

    }
}

