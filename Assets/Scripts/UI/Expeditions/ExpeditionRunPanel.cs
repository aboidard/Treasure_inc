
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExpeditionRunPanel : MonoBehaviour
{
    public Expedition expedition;
    //public int value = 0;
    //public int increment;
    //public int valueToReach;
    //public float initialPosition;

    public Rigidbody2D rb;
    public bool endMiniGame = false;
    public GameObject crewViewport;
    public GameObject crewMemberPrefab;
    private GameObject crewMember;
    public GameObject expeditionRunPanel;
    public Image touchImage;
    public GameObject targetViewport;
    private GameObject Background;
    private Event target;
    public Event[] targetAvailableList;
    public Text title;
    public bool displayTuto = true;
    public float walkSpeed = 100f;


    private void Start()
    {
        crewMember = Instantiate(crewMemberPrefab, crewViewport.transform);
        Background = Instantiate(BackgroundDatabase.instance.PickBackground(Biome.MINE), this.transform);
        Background.transform.SetSiblingIndex(0);
        crewMember.transform.SetSiblingIndex(0);

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
    }

    private void FixedUpdate()
    {
        if (!endMiniGame)
        {
            UpdateCrewPosition();
        }
        if (targetViewport.GetComponent<EventTrigger>().isInRange && !endMiniGame)
        {
            rb.velocity = Vector2.zero;
            target.Reach();
            expedition.items.AddRange(target.GetReward());
            endMiniGame = true;
            StartCoroutine(WaitForEnding());
        }
        if (displayTuto) touchImage.enabled = true;
        else touchImage.enabled = false;

    }
    private IEnumerator WaitForEnding()
    {
        yield return new WaitForSeconds(2f);

        expedition.floorOver = true;
    }


    public void UpdateCrewPosition()
    {
        crewMember.transform.position = crewViewport.transform.position;
    }

    public void Touch()
    {
        if (endMiniGame) return;
        //value += increment;
        rb.AddForce(new Vector2(walkSpeed, 0f));
        displayTuto = false;
        UpdateCrewPosition();
    }
}