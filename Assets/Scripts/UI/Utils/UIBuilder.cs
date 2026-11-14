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
