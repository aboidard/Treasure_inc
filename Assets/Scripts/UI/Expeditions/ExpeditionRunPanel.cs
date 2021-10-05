
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExpeditionRunPanel : MonoBehaviour
{
    public Expedition expedition;
    public int value = 0;
    public int increment = 100;
    public int valueToReach = 1000;
    public float initialPosition;
    public bool endMiniGame = false;
    public GameObject crewViewport;
    public GameObject crewMemberPrefab;
    private GameObject crewMember;
    public GameObject expeditionRunPanel;
    public Image touchImage;
    public GameObject targetViewport;
    private Event target;
    public Event[] targetAvailableList;
    public Text title;
    public bool displayTuto = true;


    private void Start()
    {
        float dist = Vector3.Distance(crewViewport.transform.position, targetViewport.transform.position);
        increment = (int)Math.Round(increment * dist / valueToReach);
        crewMember = Instantiate(crewMemberPrefab, crewViewport.transform);
        crewMember.transform.SetSiblingIndex(0);
        initialPosition = crewMember.transform.position.x;

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
    }

    private void FixedUpdate()
    {
        if (!endMiniGame)
        {
            UpdateCrewPosition();
        }
        if (value >= valueToReach && !endMiniGame)
        {
            target.Reach();
            expedition.items.AddRange(target.GetReward());
            touchImage.enabled = false;
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
        // Vector3 targetPosition = new Vector3(initialPosition + value, crewMember.transform.position.y, 0);
        // crewMember.transform.position = targetPosition;

        Vector3 velocity = Vector3.zero;
        //float smoothTime = 0.3F;
        Vector3 targetPosition = new Vector3(initialPosition + value, crewViewport.transform.position.y, 0);
        crewViewport.transform.position = targetPosition; //Vector3.SmoothDamp(transform.position, targetPosition , ref velocity, smoothTime);
        crewMember.transform.position = crewViewport.transform.position;
    }

    public void Touch()
    {
        if (endMiniGame) return;
        value += increment;
        displayTuto = false;
        UpdateCrewPosition();
    }
}