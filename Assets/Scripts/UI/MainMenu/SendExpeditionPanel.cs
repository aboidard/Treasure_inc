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
        if (isDiging) return;

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
        if (isDiging) return;

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
        int value = (int)Math.Round(floorSlider.value);
        // one tick per floor, higher as the expedition goes deeper
        if (value != floor) SoundManager.Play(Sfx.Tick, 0.8f + 0.04f * value);
        floor = value;
        UpdateUI();
    }

    /// <summary>
    /// An expedition needs a character to lead it: false, with a message, while they are all on an expedition.
    /// Nothing can then be dug or sent until the chest of one of their expeditions is opened.
    /// </summary>
    public bool CheckCharacterAvailable()
    {
        if (CharacterManager.Instance.AnyAvailable()) return true;

        MessagePanel.instance.DisplayMessage("Personne de disponible", "Tous vos personnages sont en expédition. Ouvrez le coffre d'une expédition revenue pour en retrouver un.");
        return false;
    }

    public void OnClickSend()
    {
        if (isDiging || ExpeditionManager.instance.isSending) return;
        if (!CheckCharacterAvailable()) return;

        if (!CharacterManager.Instance.loaded)
        {
            // the characters could not be read when the game started: try again before asking who leaves
            CharacterManager.Instance.Refresh(ok =>
            {
                if (ok) OnClickSend();
                else MessagePanel.instance.DisplayMessage("Erreur !", "Les personnages n'ont pas pu être chargés, veuillez réessayer ultérieurement !");
            });
            return;
        }

        // the user chooses who leads the expedition, then it is sent
        CharacterChooser.Show(panel.transform, locationText, panel.GetComponent<Image>().sprite, character =>
        {
            if (isDiging || ExpeditionManager.instance.isSending) return;

            int? caseId = locationButtonSelected != null ? locationButtonSelected.caseId : (int?)null;
            ExpeditionManager.instance.SendExpedition(location, floor, caseId, character, () =>
            {
                //MessagePanel.instance.DisplayMessage("Equipe envoyée !", "l'éxpédition \"" + expedition.name + "\" à été envoyée en " + location.name + " pour une durée de " + time + " mois !");
                DestroyGridLocation();
                Close();
            });
        });
    }

    private void DestroyGridLocation()
    {
        // a dig under way stops with its button
        isDiging = false;
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
            buttonScript.caseId = i;

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
        UIManager.instance.setCameraPosition(CameraMovement.POSITION_SEND);
    }
}
