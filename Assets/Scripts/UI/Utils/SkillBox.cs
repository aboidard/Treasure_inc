using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A small framed box describing a skill: a header with its name, and below what it does. A click on the header
/// folds or unfolds the description; the box is unfolded when it is created.
/// It is built by code and made for a parent with a VerticalLayoutGroup that controls the size of its children:
/// its height follows its text, and what is below moves when it is folded.
/// </summary>
public class SkillBox : MonoBehaviour
{
    private static readonly Color borderColor = new Color(0.04f, 0.05f, 0.09f);
    private static readonly Color headerColor = new Color(0.2f, 0.26f, 0.42f);
    private static readonly Color bodyColor = new Color(0.1f, 0.13f, 0.22f);
    private static readonly Color titleColor = new Color(1f, 0.93f, 0.65f);
    private static readonly Color textColor = new Color(0.9f, 0.92f, 0.96f);

    private static Sprite foldIcon, unfoldIcon;

    private GameObject body;
    private Image arrow;

    /// <param name="model">a text whose font is used</param>
    /// <param name="textSize">the size of the description; the name is a little bigger</param>
    public static SkillBox Create(Transform parent, Text model, string name, string description, int textSize)
    {
        LoadIcons();
        int border = Mathf.Max(2, textSize / 12);
        int pad = textSize / 3;
        float headerHeight = textSize * 1.6f;

        // the frame: its color shows around and between the header and the body
        RectTransform root = UIBuilder.Child("SkillBox", parent, Vector2.zero, Vector2.one);
        UIBuilder.Image(root, null, borderColor, false);
        VerticalLayoutGroup frame = Vertical(root.gameObject, border, border);
        frame.spacing = border;
        SkillBox box = root.gameObject.AddComponent<SkillBox>();

        RectTransform header = UIBuilder.Child("Header", root, Vector2.zero, Vector2.one);
        Image headerImage = UIBuilder.Image(header, null, headerColor, true);
        header.gameObject.AddComponent<LayoutElement>().preferredHeight = headerHeight;
        Button button = header.gameObject.AddComponent<Button>();
        button.targetGraphic = headerImage;
        button.onClick.AddListener(box.Toggle);

        RectTransform title = UIBuilder.Child("Name", header, Vector2.zero, Vector2.one);
        title.offsetMin = new Vector2(pad, 0f);
        title.offsetMax = new Vector2(-headerHeight, 0f);
        UIBuilder.Text(title, model, name, Mathf.RoundToInt(textSize * 1.1f), TextAnchor.MiddleLeft).color = titleColor;

        float arrowSize = headerHeight * 0.62f;
        RectTransform arrowRect = UIBuilder.Child("Arrow", header, new Vector2(1f, 0.5f),
            new Vector2(-headerHeight / 2f, 0f), new Vector2(arrowSize, arrowSize));
        box.arrow = UIBuilder.Image(arrowRect, foldIcon, titleColor, false);
        box.arrow.preserveAspect = true;

        RectTransform bodyRect = UIBuilder.Child("Body", root, Vector2.zero, Vector2.one);
        UIBuilder.Image(bodyRect, null, bodyColor, false);
        Vertical(bodyRect.gameObject, pad, pad * 2 / 3);
        box.body = bodyRect.gameObject;

        Text text = UIBuilder.Text(UIBuilder.Child("Text", bodyRect, Vector2.zero, Vector2.one),
            model, description, textSize, TextAnchor.UpperLeft);
        text.color = textColor;
        // the text keeps its size and takes the lines it needs: the box grows with it
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        // a skill without description is only a header
        if (string.IsNullOrEmpty(description))
        {
            box.body.SetActive(false);
            box.arrow.enabled = false;
            button.interactable = false;
            button.transition = Selectable.Transition.None;
        }
        return box;
    }

    public void Toggle()
    {
        bool open = !body.activeSelf;
        body.SetActive(open);
        arrow.sprite = open ? foldIcon : unfoldIcon;
    }

    // the children are as wide as the group and as high as they ask for
    private static VerticalLayoutGroup Vertical(GameObject target, int horizontalPadding, int verticalPadding)
    {
        VerticalLayoutGroup group = target.AddComponent<VerticalLayoutGroup>();
        group.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = true;
        group.childForceExpandHeight = false;
        return group;
    }

    // a disc with an arrow cut in it: up to fold the box, down to unfold it
    private static void LoadIcons()
    {
        if (foldIcon != null) return;
        string[] up =
        {
            "..#####..",
            ".#######.",
            "####.####",
            "###...###",
            "##..#..##",
            "##.###.##",
            "#########",
            ".#######.",
            "..#####..",
        };
        string[] down = (string[])up.Clone();
        System.Array.Reverse(down);
        foldIcon = UIBuilder.PixelSprite(up);
        unfoldIcon = UIBuilder.PixelSprite(down);
    }
}
