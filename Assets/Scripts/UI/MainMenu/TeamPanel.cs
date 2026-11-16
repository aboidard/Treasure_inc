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
///   Crew/Description/Text                  name, rank, stats and state of the selected character
///   ListCrewMask/ListCrew/*                the portraits of the characters 1, 2, 3... (the art of the project);
///                                          the first one is the model of the list, which holds one portrait per
///                                          character of the user and scrolls sideways
/// The "équipe" button of the main menu opens a menu (CrewMenu) leading here or to the recruitment (RecruitPanel).
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

    private readonly List<SlotView> slots = new List<SlotView>();
    private readonly List<CrewView> crew = new List<CrewView>();
    private Transform listCrew;
    private GameObject crewModel;
    private Color crewBorderColor;
    private Image photo;
    private Image photoBackground;
    private Text description;
    private int selected = 1;

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

        HookMenuButton();
        RecruitPanel.Create(panel);

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
        List<GameCharacter> characters = CharacterManager.Instance.characters;
        if (CharacterManager.Instance.Get(selected) == null && characters.Count > 0)
        {
            selected = characters[0].num;
        }
        GameCharacter character = CharacterManager.Instance.Get(selected);

        FillCrewList(characters);
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

    /// <summary>One portrait per character, in the order of their numbers; rebuilt only when the crew changed.</summary>
    private void FillCrewList(List<GameCharacter> characters)
    {
        if (crew.Select(view => view.num).SequenceEqual(characters.Select(character => character.num))) return;

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

            Text rank = UIBuilder.Text(UIBuilder.Child("Rank", member.transform, new Vector2(0.08f, 0.03f), new Vector2(0.7f, 0.24f)),
                description, character.rank, 40, TextAnchor.MiddleLeft);
            rank.color = CharacterRank.Color(character.rank);
            Outline outline = rank.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            int num = character.num;
            member.AddComponent<Button>().onClick.AddListener(() => OnClickCharacter(num));
            crew.Add(view);
        }
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
        text += "\n--------------------\n";
        if (!character.IsAvailable)
        {
            text += "<color=red>En expédition</color>\n";
        }
        return text + Equipment.DescribeTotal(character.equipment.Values, character.stats);
    }
}
