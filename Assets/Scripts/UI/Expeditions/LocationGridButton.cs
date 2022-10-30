using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LocationGridButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public List<EventLocationType> locationEvent;
    public GameObject pickaxePrefab;
    public GameObject border;
    public GameObject digCostText;
    public int digCost = 0;
    public float digTime = 0.2f;
    private bool isClicked = false;

    private void Awake()
    {
        locationEvent = new List<EventLocationType>();
        int proba = Random.Range(1, 1001);
        switch (proba)
        {
            case int n when (n <= 650):
                locationEvent.Add(EventLocationType.None);
                break;
            case int n when (n > 650 && n <= 850):
                locationEvent.Add(EventLocationType.UpToOneBonus);
                this.GetComponent<Image>().color = Color.green;
                break;
            case int n when (n > 850 && n <= 990):
                locationEvent.Add(EventLocationType.UpToTwoBonus);
                this.GetComponent<Image>().color = Color.blue;
                break;
            case int n when (n > 990 && n <= 999):
                locationEvent.Add(EventLocationType.UpToThreeBonus);
                this.GetComponent<Image>().color = Color.magenta;
                break;
            case int n when (n == 1000):
                locationEvent.Add(EventLocationType.UpTofourBonus);
                this.GetComponent<Image>().color = Color.red;
                break;
        }
        digCostText.transform.position = this.transform.position + new Vector3(0, 75, 0);
        digCostText.GetComponent<Text>().text = "- " + digCost;
    }

    public void ClickLocation()
    {
        if (SendExpeditionPanel.instance.isDiging)
        {
            return;
        }

        if (SendExpeditionPanel.instance.locationButtonSelected != null)
        {
            SendExpeditionPanel.instance.locationButtonSelected.border.GetComponent<Image>().color = Color.black;
            SendExpeditionPanel.instance.locationButtonSelected.border.SetActive(false);
        }
        SendExpeditionPanel.instance.locationButtonSelected = this;
        border.GetComponent<Image>().color = Color.red;

        if (this.isClicked)
        {
            return;
        }

        if (Inventory.Instance.CurrentMoney < digCost) throw new NotEnoughtMoneyException();
        Inventory.Instance.SubtractMoney(digCost);
        isClicked = true;
        //SendExpedition.instance.HideLocationButtons();
        SendExpeditionPanel.instance.locationButtonSelected = this;
        border.GetComponent<Image>().color = Color.red;
        digCostText.SetActive(false);
        //Pickaxe animation        
        StartCoroutine(Digging());

    }

    private IEnumerator Digging()
    {
        SendExpeditionPanel.instance.isDiging = true;
        GameObject Pickaxe = Instantiate(pickaxePrefab, this.transform);
        Pickaxe.transform.position = this.transform.position + new Vector3(25, 85, 0);
        yield return new WaitForSeconds(digTime);
        Destroy(Pickaxe);
        SendExpeditionPanel.instance.isDiging = false;

        //affichage du résultat
        DisplayResultDigging();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        border.SetActive(true);
        if (!isClicked)
        {
            digCostText.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SendExpeditionPanel.instance.locationButtonSelected != this)
        {
            border.SetActive(false);
        }
        if (!isClicked)
        {
            digCostText.SetActive(false);
        }
    }

    private void DisplayResultDigging()
    {
        var image = GetComponent<Image>();
        var tempColor = image.color;
        tempColor.a = 0.2f;
        image.color = tempColor;
    }
}
