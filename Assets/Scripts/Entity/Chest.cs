using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Chest : Event
{
    public Image lootImage;
    public Animator animator;
    public List<Item> loot = new List<Item>();
    public int size;

    public override void Start()
    {
        lootImage.enabled = false;
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

    public override void SetRewards(List<Item> loot)
    {
        this.loot.AddRange(loot);
    }
}
