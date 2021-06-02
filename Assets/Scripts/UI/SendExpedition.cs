using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SendExpedition : MonoBehaviour
{
    public static SendExpedition instance;
    public Location location;
    public Text locationText;
    public Image locationImage;
    public Text costText;
    public int time = 10;
    public Slider timeSlider;
    public Text timeText;
    public GameObject sendPanel;
    public GameObject locationGridPanel;
    public GameObject locationGridButtonPrefab;
    public LocationGridButton locationButtonSelected;
    public bool isDiging;

    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de SendExpedition dans la scène");
            return;
        }
        instance = this;
    }
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
        timeText.text = timeSlider.value.ToString() + " mois";
    }

    public void OnClickNextLocation()
    {
        int NextLocationId = location.id + 1;
        if(NextLocationId > LocationsDatabase.instance.allLocations.Count())
        {
            NextLocationId = 1;
        }
        location = LocationsDatabase.instance.allLocations.Single(x=> x.id == NextLocationId);
        
        CreateGridLocation();
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
        
        CreateGridLocation();
        UpdateUI();
    }

    public void OnValueChangedSlider()
    {
        time = (int) Math.Round(timeSlider.value);
        UpdateUI();
    }

    public void OnClickSend()
    {
        if(locationButtonSelected != null)
        {
            Debug.Log("location button Event : " + locationButtonSelected.locationEvent[0]);
        }
        Expedition expedition = ExpeditionManager.instance.SendExpedition(location, time);
        MessageManager.instance.DisplayMessage("Equipe envoyée !", "l'éxpédition \"" + expedition.name + "\" à été envoyée en " + location.name + " pour une durée de " + time + " mois !");
        DestroyGridLocation();
        ClosePanel();
    }

    private void DestroyGridLocation()
    {
        for(int i = 0; i < locationGridPanel.transform.childCount; i++)
        {
            Destroy(locationGridPanel.transform.GetChild(i).gameObject);
        }
    }
    public void CreateGridLocation()
    {
        DestroyGridLocation();
        for (var i = 0; i < 18; i++)
        {
            GameObject button = Instantiate(locationGridButtonPrefab, locationGridPanel.transform);
            LocationGridButton buttonScript = button.GetComponent<LocationGridButton>();

            button.GetComponent<Button>().onClick.AddListener(delegate{buttonScript.ClickLocation();});
        }
        HideLocationButtons();
    }

    public void HideLocationButtons()
    {   
        for (int i = 0; i < locationGridPanel.transform.childCount; i++)
        { 
            var image = locationGridPanel.transform.GetChild(i).gameObject.GetComponent<Image>();
            var tempColor = image.color;
            tempColor.a = 0f;
            image.color = tempColor;
        } 
    }

    public void OpenPanel()
    {
        sendPanel.transform.position =  new Vector3(Screen.width/2, Screen.height/2, 0);
        sendPanel.SetActive(true);
        CreateGridLocation();
    }

    public void ClosePanel()
    {
        sendPanel.SetActive(false);
    }
}
