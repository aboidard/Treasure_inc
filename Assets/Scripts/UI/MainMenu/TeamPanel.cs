using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// The crew: the characters of the user and what each of them wears. A click on a slot opens the inventory on the
/// items that fit it (see InventoryPanel.ShowForEquip).
/// The script finds its elements by name in the hierarchy of the panel:
///   Crew/Photo, Crew/Inventory/Photo_1     portrait of the selected character
///   Crew/Inventory/GameObject[_1.._8]      the 9 slots, each with an "Image" and a "Border"
///   Crew/Description/Text                  name, rank, level and state of the selected character
///   ListCrewMask/ListCrew/*                the portraits of the characters 1, 2, 3... (the art of the project);
///                                          the first one is the model of the list, which holds one portrait per
///                                          character of the user and scrolls sideways
/// Built by code: the row of sorts and filters above the list (CharacterFilterBar), the star of the portrait,
/// which makes the selected character a favorite, and the "Stats" button of the description, which opens the
/// stats and the skills of the selected character in their own screen (CharacterDetails).
/// The "équipe" button of the main menu opens a menu (CrewMenu) leading here or to the recruitment (RecruitPanel).
/// The panels built by code with the frame of this one are created here: RecruitPanel and QuestPanel.
/// </summary>
public class TeamPanel : Panel
{
    public static TeamPanel instance;

    // slot number (see Equipment) of each slot object of Crew/Inventory, by its place on the screen
    private static readonly KeyValuePair<string, int>[] slotObjects =
    {
        new KeyValuePair<string, int>("GameObject", Equipment.SLOT_HEAD),       // top
        new KeyValuePair<string, int>("GameObject_1", Equipment.SLOT_ARMOR),    // center
        new KeyValuePair<string, int>("GameObject_4", Equipment.SLOT_WEAPON),   // left
        new KeyValuePair<string, int>("GameObject_3", Equipment.SLOT_SHIELD),   // right
        new KeyValuePair<string, int>("GameObject_2", Equipment.SLOT_BOOTS),    // bottom
        new KeyValuePair<string, int>("GameObject_6", Equipment.SLOT_GLOVES),   // bottom left
        new KeyValuePair<string, int>("GameObject_5", Equipment.SLOT_RING),     // bottom right
        new KeyValuePair<string, int>("GameObject_7", Equipment.SLOT_AMULET),   // small, top left
        new KeyValuePair<string, int>("GameObject_8", Equipment.SLOT_TALISMAN), // small, top right
    };

    private static readonly Color selectedColor = new Color(1f, 0.75f, 0f);
    private static readonly Color awayColor = new Color(1f, 1f, 1f, 0.35f);

    private class SlotView
    {
        public int slot;
        public Image image;
        public Image border;
        public Text label;
        public Color emptyBorderColor;
    }

    private class CrewView
    {
        public int num;
        public GameObject member;
        public Image portrait;
        public Image border;
    }

    // just above the list of the characters, below the slots
    private static readonly Vector2 filtersPosition = new Vector2(0f, -551f);
    private static readonly Vector2 filtersSize = new Vector2(990f, 60f);
    private static readonly Color notFavoriteColor = new Color(1f, 1f, 1f, 0.45f);

    private readonly List<SlotView> slots = new List<SlotView>();
    private readonly List<CrewView> crew = new List<CrewView>();
    private Transform listCrew;
    private GameObject crewModel;
    private Color crewBorderColor;
    private Image photo;
    private Image photoBackground;
    private Text description;
    private int selected = 1;
    private CharacterListOptions options;
    private Image favoriteStar;
    private Text noMatch;
    // what the list shows: it is rebuilt when this changes
    private string crewShown;

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
        Transform root = panel.transform;
        photo = UIBuilder.Find(root, "Crew/Photo").GetComponent<Image>();
        photoBackground = UIBuilder.Find(root, "Crew/Inventory/Photo_1").GetComponent<Image>();
        // the big portrait is behind the slots: it must not take their clicks
        photoBackground.raycastTarget = false;
        description = UIBuilder.Find(root, "Crew/Description/Text").GetComponent<Text>();
        description.supportRichText = true;
        // the text leaves the bottom of its box to the button opening the stats and the skills
        RectTransform descriptionBox = description.transform.parent.GetComponent<RectTransform>();
        RectTransform descriptionText = description.rectTransform;
        descriptionText.anchorMin = new Vector2(0f, 0.32f);
        descriptionText.anchorMax = Vector2.one;
        descriptionText.offsetMin = descriptionText.offsetMax = Vector2.zero;
        UIBuilder.Button(UIBuilder.Child("Details", descriptionBox, new Vector2(0f, 0.02f), new Vector2(0.36f, 0.28f)),
            "Stats", 40, OnClickDetails);

        // the prototype was hidden behind this text
        Transform soon = root.Find("SOON");
        if (soon != null) soon.gameObject.SetActive(false);

        foreach (var slotObject in slotObjects)
        {
            Transform slot = UIBuilder.Find(root, "Crew/Inventory/" + slotObject.Key);
            SlotView view = new SlotView
            {
                slot = slotObject.Value,
                image = UIBuilder.Find(slot, "Image").GetComponent<Image>(),
                border = UIBuilder.Find(slot, "Border").GetComponent<Image>(),
            };
            view.emptyBorderColor = view.border.color;
            // the name of the slot, shown while it is empty
            view.label = UIBuilder.Text(UIBuilder.Child("Label", slot, new Vector2(0.08f, 0.3f), new Vector2(0.92f, 0.7f)),
                description, Equipment.SlotName(view.slot), 40, TextAnchor.MiddleCenter);
            int slotNumber = view.slot;
            slot.gameObject.AddComponent<Button>().onClick.AddListener(() => OnClickSlot(slotNumber));
            slots.Add(view);
        }

        listCrew = UIBuilder.Find(root, "ListCrewMask/ListCrew");
        List<Sprite> portraits = new List<Sprite>();
        for (int i = 0; i < listCrew.childCount; i++)
        {
            portraits.Add(listCrew.GetChild(i).GetComponent<Image>().sprite);
        }
        // the list is filled again from its first portrait, with the characters the user really has
        crewModel = listCrew.GetChild(0).gameObject;
        crewBorderColor = UIBuilder.Find(crewModel.transform, "Image").GetComponent<Image>().color;
        for (int i = listCrew.childCount - 1; i >= 1; i--)
        {
            Destroy(listCrew.GetChild(i).gameObject);
        }
        crewModel.SetActive(false);
        MakeCrewListScroll();

        options = CharacterListOptions.For("team");
        CharacterFilterBar.Create(UIBuilder.Child("Filters", root, new Vector2(0.5f, 0.5f), filtersPosition, filtersSize),
            options, true, Render);
        noMatch = UIBuilder.Text(UIBuilder.Child("NoMatch", listCrew.parent, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.9f)),
            description, "Aucun personnage ne correspond aux filtres.", 40, TextAnchor.MiddleCenter);
        noMatch.enabled = false;

        // the star of the big portrait: the selected character is a favorite, or not
        RectTransform star = UIBuilder.Child("Favorite", photo.transform, new Vector2(0.7f, 0.76f), new Vector2(0.99f, 0.99f));
        favoriteStar = UIBuilder.Image(star, CharacterFilterBar.Star, notFavoriteColor, true);
        favoriteStar.preserveAspect = true;
        Shadow starShadow = star.gameObject.AddComponent<Shadow>();
        starShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        starShadow.effectDistance = new Vector2(3f, -3f);
        Button favorite = star.gameObject.AddComponent<Button>();
        favorite.targetGraphic = favoriteStar;
        favorite.onClick.AddListener(OnClickFavorite);

        HookMenuButton();
        RecruitPanel.Create(panel);
        QuestPanel.Create(panel);
        GiftPanel.Create(panel);

        CharacterManager.Instance.SetPortraits(portraits);
        CharacterManager.Instance.Refresh(ok =>
        {
            if (!ok) return;
            ExpeditionManager.instance.RecoverLostExpeditions();
            if (IsOpen()) Render();
        });
    }

    /// <summary>The characters stay on one row, which scrolls sideways once it is wider than the panel.</summary>
    private void MakeCrewListScroll()
    {
        RectTransform content = listCrew.GetComponent<RectTransform>();
        RectTransform viewport = listCrew.parent.GetComponent<RectTransform>();

        GridLayoutGroup grid = content.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
        content.anchorMin = content.anchorMax = new Vector2(0f, 0.5f);
        content.pivot = new Vector2(0f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = true;
        scroll.vertical = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
    }

    /// <summary>
    /// The button of the main menu was bound to Show in the scene: it now opens the menu of the crew, from which
    /// this panel and the recruitment are opened.
    /// </summary>
    private void HookMenuButton()
    {
        foreach (Button button in FindObjectsOfType<Button>(true))
        {
            Button.ButtonClickedEvent click = button.onClick;
            for (int i = 0; i < click.GetPersistentEventCount(); i++)
            {
                if (click.GetPersistentTarget(i) != this || click.GetPersistentMethodName(i) != "Show") continue;
                click.SetPersistentListenerState(i, UnityEventCallState.Off);
                RectTransform anchor = button.GetComponent<RectTransform>();
                click.AddListener(() => CrewMenu.Toggle(anchor));
            }
        }
    }

    /// <summary>Opens the panel, unless it is already open.</summary>
    public void OpenTeam()
    {
        if (!IsOpen()) Show();
    }

    protected override void WillShow()
    {
        UIManager.instance.setCameraPosition(CameraMovement.POSITION_CREW);
        Render();
        if (!CharacterManager.Instance.loaded)
        {
            CharacterManager.Instance.Refresh(ok => { if (ok && IsOpen()) Render(); });
        }
    }

    public void OnClickCharacter(int num)
    {
        selected = num;
        Render();
    }

    public void OnClickFavorite()
    {
        if (CharacterManager.Instance.Get(selected) == null) return;
        CharacterFavorites.Toggle(selected);
        Render();
    }

    public void OnClickDetails()
    {
        CharacterDetails.Show(CharacterManager.Instance.Get(selected), panel.transform);
    }

    public void OnClickSlot(int slot)
    {
        GameCharacter character = CharacterManager.Instance.Get(selected);
        if (character == null)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", "Les personnages n'ont pas pu être chargés, veuillez réessayer ultérieurement !");
            return;
        }
        if (!character.IsAvailable)
        {
            MessagePanel.instance.DisplayMessage("En expédition", character.name + " est en expédition : son équipement ne peut pas être changé avant son retour.");
            return;
        }
        InventoryPanel.instance.ShowForEquip(character, slot);
    }

    public void RenderIfOpen()
    {
        if (IsOpen()) Render();
    }

    /// <summary>Shows the characters and the selected one again, e.g. after its equipment changed or a recruitment.</summary>
    public void Render()
    {
        List<GameCharacter> characters = options.Apply(CharacterManager.Instance.characters);
        // the selected character is one of the list, as long as the filters leave some
        if (characters.Count > 0 && !characters.Any(shown => shown.num == selected))
        {
            selected = characters[0].num;
        }
        else if (CharacterManager.Instance.Get(selected) == null && CharacterManager.Instance.characters.Count > 0)
        {
            selected = CharacterManager.Instance.characters[0].num;
        }
        GameCharacter character = CharacterManager.Instance.Get(selected);

        FillCrewList(characters);
        noMatch.enabled = characters.Count == 0 && CharacterManager.Instance.characters.Count > 0;
        favoriteStar.color = character != null && character.IsFavorite ? CharacterFilterBar.FavoriteColor : notFavoriteColor;
        foreach (CrewView view in crew)
        {
            GameCharacter member = CharacterManager.Instance.Get(view.num);
            view.border.color = view.num == selected ? selectedColor : crewBorderColor;
            view.portrait.color = member != null && !member.IsAvailable ? awayColor : Color.white;
        }

        if (character != null && character.graphics != null)
        {
            photo.sprite = character.graphics;
            photoBackground.sprite = character.graphics;
        }

        foreach (SlotView view in slots)
        {
            Item item = character != null ? character.Equipped(view.slot) : null;
            view.image.enabled = item != null;
            view.label.enabled = item == null;
            if (item != null)
            {
                view.image.sprite = item.graphics;
                view.border.color = Item.getRarityColor(item.rarity);
            }
            else
            {
                view.border.color = view.emptyBorderColor;
            }
        }

        description.text = Describe(character);
    }

    /// <summary>
    /// One portrait per character, in the order given, with its rank, its level and a star when it is a favorite;
    /// rebuilt only when one of these changed.
    /// </summary>
    private void FillCrewList(List<GameCharacter> characters)
    {
        string shown = string.Join(";", characters.Select(character =>
            character.num + ":" + character.level + ":" + (character.IsFavorite ? "f" : "")));
        if (shown == crewShown) return;
        crewShown = shown;

        foreach (CrewView view in crew)
        {
            Destroy(view.member);
        }
        crew.Clear();
        foreach (GameCharacter character in characters)
        {
            GameObject member = Instantiate(crewModel, listCrew);
            member.name = "CrewMember" + character.num;
            member.SetActive(true);
            CrewView view = new CrewView
            {
                num = character.num,
                member = member,
                portrait = member.GetComponent<Image>(),
                border = UIBuilder.Find(member.transform, "Image").GetComponent<Image>(),
            };
            view.portrait.sprite = character.graphics;

            Text rank = UIBuilder.Text(UIBuilder.Child("Rank", member.transform, new Vector2(0.08f, 0.03f), new Vector2(0.5f, 0.24f)),
                description, character.rank, 40, TextAnchor.MiddleLeft);
            rank.color = CharacterRank.Color(character.rank);
            Outlined(rank);
            Text level = UIBuilder.Text(UIBuilder.Child("Level", member.transform, new Vector2(0.5f, 0.03f), new Vector2(0.92f, 0.24f)),
                description, "Nv " + character.level, 34, TextAnchor.MiddleRight);
            level.color = Color.white;
            Outlined(level);
            if (character.IsFavorite)
            {
                Image star = UIBuilder.Image(UIBuilder.Child("Favorite", member.transform, new Vector2(0.68f, 0.72f), new Vector2(0.94f, 0.96f)),
                    CharacterFilterBar.Star, CharacterFilterBar.FavoriteColor, false);
                star.preserveAspect = true;
                Outline starOutline = star.gameObject.AddComponent<Outline>();
                starOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
                starOutline.effectDistance = new Vector2(2f, -2f);
            }

            int num = character.num;
            member.AddComponent<Button>().onClick.AddListener(() => OnClickCharacter(num));
            crew.Add(view);
        }
    }

    private static void Outlined(Text text)
    {
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);
    }

    private static string Describe(GameCharacter character)
    {
        if (character == null)
        {
            return "Chargement...";
        }
        string text = character.name + "\nRang " + CharacterRank.Colored(character.rank);
        if (character.copies > 1)
        {
            text += "   x" + character.copies;
        }
        text += "\n" + character.LevelText;
        if (!character.IsAvailable)
        {
            text += "\n<color=red>En expédition</color>";
        }
        return text;
    }
}
