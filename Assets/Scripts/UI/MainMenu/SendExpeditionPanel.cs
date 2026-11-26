using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SendExpeditionPanel : Panel
{
    public Location location;
    public Text locationText;
    /// <summary>The difficulty of the location (its level) as stars, made under locationText.</summary>
    private RectTransform difficultyStars;
    public Image locationImage;
    public Text costText;
    /// <summary>How many floors an expedition of the story goes down: always the most.</summary>
    public const int Floors = 10;
    public int floor = Floors;
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
        // the expeditions of the story always go down every floor: there is no length to choose any more
        floor = Floors;
        if (floorSlider != null) floorSlider.gameObject.SetActive(false);
        if (floorText != null) floorText.gameObject.SetActive(false);

        locationText.text = location.name;
        locationImage.sprite = location.graphics;
        if (difficultyStars == null) difficultyStars = CreateDifficultyStars();
        DifficultyStars.Show(difficultyStars, location.level);
        int cost = ExpeditionManager.instance.ComputeCost(floor, location);
        costText.text = cost == 0 ? "Gratuit !" : "- " + cost;
    }

    /// <summary>The difficulty of the location as stars, under its name.</summary>
    private RectTransform CreateDifficultyStars()
    {
        RectTransform nameRect = locationText.rectTransform;
        float width = nameRect.anchorMax.x - nameRect.anchorMin.x;
        float height = nameRect.anchorMax.y - nameRect.anchorMin.y;
        // slightly raised, so that they sit right under the text of the name rather than under its box
        float raise = height * 1.5f;
        RectTransform stars = DifficultyStars.Create(locationText.transform.parent,
            new Vector2(nameRect.anchorMin.x + width * 0.3f, nameRect.anchorMin.y - height * 0.5f + raise),
            new Vector2(nameRect.anchorMax.x - width * 0.3f, nameRect.anchorMin.y + raise), location.level);
        return stars;
    }

    /// <summary>
    /// The button of the menu: it first asks where to go, the story (the locations of this panel) or the events,
    /// in a small menu above the button (like the one of the crew).
    /// </summary>
    public void OpenExpeditionMenu()
    {
        if (IsOpen())
        {
            Show();
            return;
        }
        RectTransform anchor = FindMenuButton();
        if (anchor == null)
        {
            Show();
            return;
        }
        ExpeditionMenu.Toggle(anchor, Show, EventsWindow.Show);
    }

    /// <summary>The button of the main menu bound to OpenExpeditionMenu in the scene: the menu opens above it.</summary>
    private RectTransform FindMenuButton()
    {
        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            Button.ButtonClickedEvent click = button.onClick;
            for (int i = 0; i < click.GetPersistentEventCount(); i++)
            {
                if (click.GetPersistentTarget(i) == this && click.GetPersistentMethodName(i) == "OpenExpeditionMenu")
                {
                    return button.GetComponent<RectTransform>();
                }
            }
        }
        return null;
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
