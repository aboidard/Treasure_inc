using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates the few UI elements that are built by code instead of being laid out in a scene or a prefab.</summary>
public static class UIBuilder
{
    /// <summary>A child filling the rectangle between two anchors of its parent (0..1 on each axis).</summary>
    public static RectTransform Child(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    /// <summary>A child of a fixed size, centered on a point of its parent.</summary>
    public static RectTransform Child(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
    {
        RectTransform rect = Child(name, parent, anchor, anchor);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    public static Image Image(RectTransform rect, Sprite sprite, Color color, bool raycastTarget)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = raycastTarget;
        return image;
    }

    /// <summary>A text that shrinks to fit its rectangle, with the font, style and color of another text.</summary>
    public static Text Text(RectTransform rect, Text model, string content, int maxSize, TextAnchor alignment)
    {
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = model.font;
        text.fontStyle = model.fontStyle;
        text.color = model.color;
        text.text = content;
        text.alignment = alignment;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 10;
        text.resizeTextMaxSize = maxSize;
        text.fontSize = maxSize;
        return text;
    }

    // The look of what is built by code comes from the inventory panel of the scene: its font, its buttons, its frame.

    /// <summary>A text of the scene whose font and style are used for the texts built by code.</summary>
    public static Text ModelText => InventoryPanel.instance.title;

    /// <summary>The frame of the panels of the scene.</summary>
    public static Image ModelPanel => InventoryPanel.instance.panel.GetComponent<Image>();

    private static Image ModelButton => InventoryPanel.instance.autoSelectButton.GetComponent<Image>();

    /// <summary>The color of the texts written on a button.</summary>
    public static Color ButtonTextColor
    {
        get
        {
            Text[] texts = InventoryPanel.instance.autoSelectButton.GetComponentsInChildren<Text>(true);
            return texts.Length > 0 ? texts[0].color : Color.black;
        }
    }

    /// <summary>A button with the look of those of the scene; its label may hold several lines.</summary>
    public static Button Button(RectTransform rect, string label, int maxSize, UnityEngine.Events.UnityAction onClick)
    {
        Image image = Image(rect, ModelButton.sprite, ModelButton.color, true);
        image.type = ModelButton.type;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        Text text = Text(Child("Text", rect, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.88f)),
            ModelText, label, maxSize, TextAnchor.MiddleCenter);
        text.color = ButtonTextColor;
        return button;
    }

    /// <summary>A number with its thousands separated by a space, as the money is shown.</summary>
    public static string Number(int value)
    {
        var format = (System.Globalization.NumberFormatInfo)System.Globalization.CultureInfo.InvariantCulture.NumberFormat.Clone();
        format.NumberGroupSeparator = " ";
        return value.ToString("#,0", format);
    }

    /// <summary>
    /// Keeps a text on one line, shrinking its font until it fits the width of its rectangle. The "best fit" of
    /// Unity cuts a word that is too long in two lines instead.
    /// </summary>
    public static void FitOneLine(Text text)
    {
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        // a line taller than the rectangle would hide the text instead of overflowing
        text.verticalOverflow = VerticalWrapMode.Overflow;
        float available = text.rectTransform.rect.width;
        float needed = text.preferredWidth;
        if (available > 0 && needed > available)
        {
            text.fontSize = Mathf.Max(1, Mathf.FloorToInt(text.fontSize * available / needed));
        }
    }

    /// <summary>
    /// A small white icon drawn pixel by pixel, to be tinted by its Image: one string per row from the top,
    /// '#' for a pixel. The icons do not depend on the glyphs of the font this way.
    /// </summary>
    public static Sprite PixelSprite(params string[] rows)
    {
        int height = rows.Length;
        int width = rows[0].Length;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color clear = new Color(1f, 1f, 1f, 0f);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // the first row of a texture is its bottom
                texture.SetPixel(x, height - 1 - y, rows[y][x] == '#' ? Color.white : clear);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>The transform at a path below a root; logs what is missing instead of failing later on a null.</summary>
    public static Transform Find(Transform root, string path)
    {
        Transform found = root.Find(path);
        if (found == null)
        {
            Debug.LogError("\"" + path + "\" not found under " + root.name + " : was the scene changed ?");
        }
        return found;
    }
}
