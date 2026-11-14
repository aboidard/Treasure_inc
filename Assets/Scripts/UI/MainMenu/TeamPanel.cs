using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The crew: the characters of the user and what each of them wears. A click on a slot opens the inventory on the
/// items that fit it (see InventoryPanel.ShowForEquip).
/// The script finds its elements by name in the hierarchy of the panel:
///   Crew/Photo, Crew/Inventory/Photo_1     portrait of the selected character
///   Crew/Inventory/GameObject[_1.._8]      the 9 slots, each with an "Image" and a "Border"
///   Crew/Description/Text                  name, stats and state of the selected character
///   ListCrewMask/ListCrew/*                one portrait per character, in the order of their numbers
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
        public Image portrait;
        public Image border;
        public Color borderColor;
    }

    private readonly List<SlotView> slots = new List<SlotView>();
    private readonly List<CrewView> crew = new List<CrewView>();
    private Image photo;
    private Image photoBackground;
    private float photoBackgroundAlpha;
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
        photoBackgroundAlpha = photoBackground.color.a;
        // the big portrait is behind the slots: it must not take their clicks
        photoBackground.raycastTarget = false;
        description = UIBuilder.Find(root, "Crew/Description/Text").GetComponent<Text>();

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

        Transform listCrew = UIBuilder.Find(root, "ListCrewMask/ListCrew");
        List<Sprite> portraits = new List<Sprite>();
        for (int i = 0; i < listCrew.childCount; i++)
        {
            Transform member = listCrew.GetChild(i);
            CrewView view = new CrewView
            {
                portrait = member.GetComponent<Image>(),
                border = UIBuilder.Find(member, "Image").GetComponent<Image>(),
            };
            view.borderColor = view.border.color;
            portraits.Add(view.portrait.sprite);
            int num = i + 1;
            member.gameObject.AddComponent<Button>().onClick.AddListener(() => OnClickCharacter(num));
            crew.Add(view);
        }

        CharacterManager.Instance.SetPortraits(portraits);
        CharacterManager.Instance.Refresh(ok =>
        {
            if (!ok) return;
            ExpeditionManager.instance.RecoverLostExpeditions();
            if (IsOpen()) Render();
        });
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

    /// <summary>Shows the selected character again, e.g. after its equipment changed.</summary>
    public void Render()
    {
        GameCharacter character = CharacterManager.Instance.Get(selected);

        for (int i = 0; i < crew.Count; i++)
        {
            GameCharacter member = CharacterManager.Instance.Get(i + 1);
            crew[i].border.color = i + 1 == selected ? selectedColor : crew[i].borderColor;
            crew[i].portrait.color = member != null && !member.IsAvailable ? awayColor : Color.white;
        }

        Sprite portrait = selected >= 1 && selected <= crew.Count ? crew[selected - 1].portrait.sprite : null;
        if (portrait != null)
        {
            photo.sprite = portrait;
            photoBackground.sprite = portrait;
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

    private static string Describe(GameCharacter character)
    {
        if (character == null)
        {
            return "Chargement...";
        }
        string text = character.name + "\n--------------------\n";
        if (!character.IsAvailable)
        {
            text += "<color=red>En expédition</color>\n";
        }
        return text + Equipment.DescribeTotal(character.equipment.Values);
    }
}
