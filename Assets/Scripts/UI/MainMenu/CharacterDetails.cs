using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The details of a character: its rank, its level, what it is worth in a fight, its stats and its skills, those
/// of its equipment included, each in a box that can be folded (SkillBox). Opened by the "Stats" button of the crew panel (TeamPanel), which only shows the
/// name, the rank and the level. It is built by code on top of the canvas; its content scrolls when it is longer
/// than the screen. A click outside of it, or on its button, closes it.
/// </summary>
public class CharacterDetails : MonoBehaviour
{
    private static CharacterDetails instance;

    private const int textSize = 38;
    private const int headingSize = 46;

    private Text title;
    private Text body;
    private RectTransform content;
    private readonly List<SkillBox> skillBoxes = new List<SkillBox>();

    public static void Show(GameCharacter character, Transform from)
    {
        if (character == null) return;
        if (instance == null)
        {
            Canvas canvas = from.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("CharacterDetails", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<CharacterDetails>();
            instance.Build();
        }
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        instance.Fill(character);
        SoundManager.Play(Sfx.PanelOpen, 1.3f);
    }

    private void Build()
    {
        // darkens the screen and takes the clicks made outside of the box
        Image blocker = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0f, 0f, 0f, 0.6f), true);
        Button outside = gameObject.AddComponent<Button>();
        outside.targetGraphic = blocker;
        outside.transition = Selectable.Transition.None;
        outside.onClick.AddListener(Close);

        RectTransform box = UIBuilder.Child("Box", transform, new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f));
        Image frame = UIBuilder.Image(box, UIBuilder.ModelPanel.sprite, Color.white, true);
        frame.type = UIBuilder.ModelPanel.type;

        Text model = UIBuilder.ModelText;
        title = UIBuilder.Text(UIBuilder.Child("Title", box, new Vector2(0.06f, 0.9f), new Vector2(0.94f, 0.975f)),
            model, "", 64, TextAnchor.MiddleCenter);

        // the transparent image takes the drags that scroll the text
        RectTransform viewport = UIBuilder.Child("Viewport", box, new Vector2(0.07f, 0.13f), new Vector2(0.93f, 0.89f));
        UIBuilder.Image(viewport, null, new Color(0f, 0f, 0f, 0f), true);
        viewport.gameObject.AddComponent<RectMask2D>();

        // as wide as the viewport, as high as what it holds, hanging from the top: the text, then a box for each skill
        content = UIBuilder.Child("Content", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = textSize * 0.4f;
        layout.padding = new RectOffset(0, 0, 0, textSize / 2);
        body = UIBuilder.Text(UIBuilder.Child("Text", content, Vector2.zero, Vector2.one), model, "", textSize, TextAnchor.UpperLeft);
        body.resizeTextForBestFit = false;
        body.horizontalOverflow = HorizontalWrapMode.Wrap;
        body.verticalOverflow = VerticalWrapMode.Overflow;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        UIBuilder.Button(UIBuilder.Child("Close", box, new Vector2(0.3f, 0.03f), new Vector2(0.7f, 0.105f)),
            "Fermer", 46, Close);
    }

    private void Fill(GameCharacter character)
    {
        title.text = character.name;
        body.text = Describe(character);

        foreach (SkillBox box in skillBoxes)
        {
            // destroyed at the end of the frame: hidden until then, so the layout forgets it at once
            box.gameObject.SetActive(false);
            Destroy(box.gameObject);
        }
        skillBoxes.Clear();
        foreach (string skill in Equipment.TotalSkills(character.equipment.Values, character.skills))
        {
            skillBoxes.Add(SkillBox.Create(content, body, Equipment.SkillName(skill), Equipment.SkillEffect(skill), textSize));
        }
        // back to the top, whatever was read before
        content.anchoredPosition = Vector2.zero;
    }

    private static string Describe(GameCharacter character)
    {
        List<string> lines = new List<string>();
        string rank = "Rang " + CharacterRank.Colored(character.rank);
        if (character.copies > 1) rank += "   x" + character.copies;
        lines.Add(rank);
        lines.Add(character.LevelText);
        if (!character.IsAvailable) lines.Add("<color=red>En expédition</color>");

        if (character.combat != null)
        {
            lines.Add(Heading("Combat"));
            lines.Add(character.combat.Describe());
        }

        lines.Add(Heading("Caractéristiques"));
        lines.Add(Equipment.DescribeTotalStats(character.equipment.Values, character.stats));

        // the skills follow the text, each in its box (see Fill)
        lines.Add(Heading("Compétences"));
        if (Equipment.TotalSkills(character.equipment.Values, character.skills).Count == 0) lines.Add("Aucune");
        return string.Join("\n", lines);
    }

    private static string Heading(string text)
    {
        return "\n<size=" + headingSize + ">" + text.ToUpper() + "</size>";
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SoundManager.Play(Sfx.PanelClose);
    }
}
