using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootButton : MonoBehaviour
{
    public GameObject lootButton;
    public Expedition ex;

    public void onClick()
    {
        LootPanel.instance.ShowLoot(ex.expeditionId, "l'expedition \"" + ex.expeditionName + "\" est de retour !");
        Destroy(lootButton);
    }
}
