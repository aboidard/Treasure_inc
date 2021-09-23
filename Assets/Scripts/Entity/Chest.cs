using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Chest : Event
{
    public Image lootImage;
    public Animator animator;
    public List<Item> loot;
    public int size;

    public override void Start()
    {
        lootImage.enabled = false;
        loot = new List<Item>();
    }

    public override bool Reach()
    {
        animator.SetBool("Open", true);
        lootImage.enabled = true;
        return true;
    }

    public override List<Item> GetReward()
    {
        return loot;
    }

    public override void SetReward(List<Item> loot)
    {
        this.loot.AddRange(loot);
    }
}
