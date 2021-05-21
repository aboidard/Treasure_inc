using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SendExpedition : MonoBehaviour
{
    public Location location;
    public Text locationText;
    public Image locationImage;
    public Text costText;
    public int time = 10;
    public Slider timeSlider;
    public Text timeText;
    public GameObject sendPanel;

    private void Start() 
    {
        UpdateUI();
        ClosePanel();
    }

    public void UpdateUI()
    {
        locationText.text = location.name;
        locationImage.sprite = location.graphics;
        costText.text = "- " + ExpeditionManager.instance.ComputeCost(time, location);
        timeSlider.value = time;
        timeText.text = timeSlider.value.ToString();
    }

    public void OnClickNextLocation()
    {
        int NextLocationId = location.id + 1;
        if(NextLocationId > LocationsDatabase.instance.allLocations.Count())
        {
            NextLocationId = 1;
        }
        location = LocationsDatabase.instance.allLocations.Single(x=> x.id == NextLocationId);
        UpdateUI();
    }
    public void OnClickPreviousLocation()
    {
        int previousLocationId = location.id - 1;
        if(previousLocationId == 0)
        {
            previousLocationId = LocationsDatabase.instance.allLocations.Count();
        }
        location = LocationsDatabase.instance.allLocations.Single(x=> x.id == previousLocationId);
        UpdateUI();
    }

    public void OnValueChangedSlider()
    {
        time = (int) Math.Round(timeSlider.value);
        UpdateUI();
    }

    public void OnClickSend()
    {
        Expedition expedition = ExpeditionManager.instance.SendExpedition(location, time);
        MessageManager.instance.DisplayMessage("Equipe envoyée !", "l'éxpédition \"" + expedition.name + "\" à été envoyée en " + location.name + " pour une durée de " + time + " mois !");
        ClosePanel();
    }

    public void OpenPanel()
    {
        sendPanel.transform.position =  new Vector3(Screen.width/2, Screen.height/2, 0);
        sendPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        sendPanel.SetActive(false);
    }
}
