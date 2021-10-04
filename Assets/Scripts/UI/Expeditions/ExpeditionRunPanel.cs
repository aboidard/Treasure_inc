
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExpeditionRunPanel : MonoBehaviour
{
    public Expedition expedition;
    public int value = 0;
    public int valueToReach;
    public float initialPosition;
    public bool endMiniGame = false;
    public GameObject crewViewport;
    public GameObject crewMemberPrefab;
    private GameObject crewMember;
    public GameObject expeditionRunPanel;
    public Image touchImage;
    public Sprite chestClose;
    public Sprite chestOpen;
    public GameObject targetViewport;
    private Event target;
    public Event[] targetAvailableList;
    public Text title;
    public bool displayTuto = true;


    private void Start()
    {
        float panelWidth = expeditionRunPanel.GetComponent<RectTransform>().sizeDelta.x;
        crewMember = Instantiate(crewMemberPrefab, crewViewport.transform);
        crewMember.transform.SetSiblingIndex(0);
        initialPosition = crewMember.transform.position.x;

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
    }

    private void FixedUpdate()
    {
        if (value >= 1 && !endMiniGame)
        {
            value -= 1;
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
        value += 100;
        displayTuto = false;
        UpdateCrewPosition();
    }
}