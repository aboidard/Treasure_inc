using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SendExpeditionPanel : Panel
{
    public Location location;
    public Text locationText;
    public Image locationImage;
    public Text costText;
    public int floor = 10;
    public Slider floorSlider;
    public Text floorText;
    public GameObject locationGridPanel;
    public GameObject locationGridButtonPrefab;
    public LocationGridButton locationButtonSelected;
    public bool isDiging;
    public static SendExpeditionPanel instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    protected override void WillStart()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        locationText.text = location.name;
        locationImage.sprite = location.graphics;
        costText.text = "- " + ExpeditionManager.instance.ComputeCost(floor, location);
        floorSlider.value = floor;
        floorText.text = floorSlider.value.ToString() + (floorSlider.value != 1 ? " étages" : " étage");
    }

    public void OnClickNextLocation()
    {
        int NextLocationId = location.id + 1;
        if (NextLocationId > LocationsDatabase.instance.allLocations.Count())
        {
            NextLocationId = 1;
        }
        location = LocationsDatabase.instance.allLocations.Single(x => x.id == NextLocationId);

        CreateGridLocation();
        UpdateUI();
    }
    public void OnClickPreviousLocation()
    {
        int previousLocationId = location.id - 1;
        if (previousLocationId == 0)
        {
            previousLocationId = LocationsDatabase.instance.allLocations.Count();
        }
        location = LocationsDatabase.instance.allLocations.Single(x => x.id == previousLocationId);

        CreateGridLocation();
        UpdateUI();
    }

    public void OnValueChangedSlider()
    {
        floor = (int)Math.Round(floorSlider.value);
        UpdateUI();
    }

    public void OnClickSend()
    {
        if (locationButtonSelected != null)
        {
            Debug.Log("location button Event : " + locationButtonSelected.locationEvent[0]);
        }
        Expedition expedition = ExpeditionManager.instance.SendExpedition(location, floor);
        //MessagePanel.instance.DisplayMessage("Equipe envoyée !", "l'éxpédition \"" + expedition.name + "\" à été envoyée en " + location.name + " pour une durée de " + time + " mois !");
        DestroyGridLocation();
        Close();
    }

    private void DestroyGridLocation()
    {
        for (int i = 0; i < locationGridPanel.transform.childCount; i++)
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

            button.GetComponent<Button>().onClick.AddListener(delegate { buttonScript.ClickLocation(); });
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

    protected override void WillShow()
    {
        CreateGridLocation();
    }
}
