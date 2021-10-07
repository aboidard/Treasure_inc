
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExpeditionPanel : MonoBehaviour
{
    public Expedition expedition;
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
        float panelWidth = expeditionRunPanel.GetComponent<RectTransform>().sizeDelta.x;
        crewMember = Instantiate(crewMemberPrefab, crewViewport.transform);
        crewMember.transform.SetSiblingIndex(0);
        initialPosition = crewMember.transform.position.x;

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
    }

    private void FixedUpdate()
    {
        //StartCoroutine(WaitForEnding());
    }
    private IEnumerator WaitForEnding()
    {
        yield return new WaitForSeconds(2f);

        expedition.floorOver = true;
    }
}