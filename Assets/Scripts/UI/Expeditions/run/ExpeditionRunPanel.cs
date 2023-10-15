
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExpeditionRunPanel : MonoBehaviour
{
    public Expedition expedition;
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
    public Text lootNumberText;
    public bool displayTuto = true;
    public float walkSpeed = 7000f;
    public float initialMagnitude = 140f;
    public int size = ExpeditionGridPanel.SIZE_S;

    private void Start()
    {
        crewMember = Instantiate(crewMemberPrefab, crewViewport.transform);
        Background = Instantiate(BackgroundDatabase.instance.PickBackground(Biome.MINE), this.transform);
        Background.transform.SetSiblingIndex(0);
        crewMember.transform.SetSiblingIndex(0);
        lootNumberText.text = "0";

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
        initialMagnitude = 140f;
        rb.AddForce(new Vector2(walkSpeed, 0f));
        updateUI();
    }

    private void FixedUpdate()
    {
        if (!endMiniGame)
        {
            UpdateCrewPosition();
        }
        if (targetViewport.GetComponent<EventTrigger>().isInRange && !endMiniGame)
        {
            //stop the crew
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            target.Reach();
            expedition.items.AddRange(target.GetReward());
            endMiniGame = true;
            updateUI();
            StartCoroutine(WaitForEnding());
        }
        if (displayTuto) touchImage.enabled = true;
        else touchImage.enabled = false;

    }
    private IEnumerator WaitForEnding()
    {
        yield return new WaitForSeconds(1.5f);

        expedition.floorOver = true;
    }


    public void UpdateCrewPosition()
    {
        crewMember.transform.position = crewViewport.transform.position;
        //if the crew velocity is > walkSpeed, then the velocity is gradualy reduced to walkSpeed

        if (rb.velocity.magnitude > initialMagnitude)
        {
            rb.AddForce(new Vector2(-walkSpeed / 60, 0f));
        }
    }

    public void updateUI()
    {
        title.text = expedition.expeditionName + " - étage " + expedition.currentFloor + "/" + expedition.nbTtotalFloor;
        Debug.Log("lootNumberText " + lootNumberText + " expedition.items " + expedition.items);
        lootNumberText.text = expedition.items.Count.ToString();
    }

    public void Touch()
    {
        if (endMiniGame) return;
        rb.AddForce(new Vector2(walkSpeed, 0f));
        displayTuto = false;
        UpdateCrewPosition();
    }

    public void SetExpedition(Expedition expedition)
    {
        this.expedition = expedition;
        updateUI();
    }
}