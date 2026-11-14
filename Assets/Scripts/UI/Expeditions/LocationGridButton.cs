using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

public class LocationGridButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int caseId;
    public GameObject pickaxePrefab;
    public GameObject border;
    public GameObject digCostText;
    public int digCost = 0;
    public float digTime = 0.2f;
    private bool isClicked = false;

    private void Awake()
    {
        digCostText.transform.position = this.transform.position + new Vector3(0, 40, 0);
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

        //SendExpedition.instance.HideLocationButtons();
        digCostText.SetActive(false);
        //Pickaxe animation
        StartCoroutine(Digging());

    }

    private IEnumerator Digging()
    {
        SendExpeditionPanel.instance.isDiging = true;
        GameObject Pickaxe = Instantiate(pickaxePrefab, this.transform);
        Pickaxe.transform.position = this.transform.position + new Vector3(25, 85, 0);

        float start = Time.time;
        long status = 0;
        string response = null;
        DigCaseRequest body = new DigCaseRequest { location = SendExpeditionPanel.instance.location.name, caseId = caseId, cost = digCost };
        yield return NetworkManager.Instance.PostJson(NetworkManager.digCaseEndPoint, body, (code, text) =>
        {
            status = code;
            response = text;
        });
        // the animation lasts at least digTime, however fast the API answers
        float remaining = digTime - (Time.time - start);
        if (remaining > 0)
        {
            yield return new WaitForSeconds(remaining);
        }
        Destroy(Pickaxe);
        SendExpeditionPanel.instance.isDiging = false;

        if (status == 200)
        {
            DigCaseResponse dug = JsonConvert.DeserializeObject<DigCaseResponse>(response);
            Inventory.Instance.CurrentMoney = dug.money;
            isClicked = true;
            //affichage du résultat
            DisplayResultDigging((EventLocationType)dug.rarity);
        }
        else
        {
            Unselect();
            if (status == 402)
            {
                MessagePanel.instance.DisplayMessage("Pas assez d'argent !", "Vous n'avez pas assez d'argent pour creuser ici.");
            }
            else
            {
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de creuser, veuillez réessayer ultérieurement !\n" + response);
            }
        }
    }

    private void Unselect()
    {
        SendExpeditionPanel.instance.locationButtonSelected = null;
        border.GetComponent<Image>().color = Color.black;
        border.SetActive(false);
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

    private void DisplayResultDigging(EventLocationType rarity)
    {
        var image = GetComponent<Image>();
        var tempColor = RarityColor(rarity, image.color);
        tempColor.a = 0.2f;
        image.color = tempColor;
    }

    private static Color RarityColor(EventLocationType rarity, Color noRarity)
    {
        switch (rarity)
        {
            case EventLocationType.UpToOneBonus: return Color.green;
            case EventLocationType.UpToTwoBonus: return Color.blue;
            case EventLocationType.UpToThreeBonus: return Color.magenta;
            case EventLocationType.UpTofourBonus: return Color.red;
            default: return noRarity;
        }
    }
}
