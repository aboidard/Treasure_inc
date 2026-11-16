using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The tooltip shown next to an item while a slot is being filled: what the item changes compared to the one
/// in the slot, and a button to equip it.
///   green chevron up / red chevron down    a stat that goes up / down
///   green check / red cross                a skill gained / lost
/// It is built by code, as a child of the panel that shows it. A click on it closes it.
/// </summary>
public class EquipTooltip
{
    private static readonly Color goodColor = new Color(0.35f, 0.9f, 0.4f);
    private static readonly Color badColor = new Color(1f, 0.35f, 0.35f);
    private static readonly Color backgroundColor = new Color(0.06f, 0.08f, 0.14f, 0.96f);
    private static readonly Color secondaryColor = new Color(0.8f, 0.8f, 0.8f);

    private static Sprite chevronUp, chevronDown, check, cross;

    private readonly RectTransform parent;
    private readonly Text textModel;
    private readonly Image buttonModel;
    private readonly Color buttonTextColor;
    private GameObject root;
    private Item shown;

    /// <param name="parent">the panel the tooltip is placed in</param>
    /// <param name="textModel">gives the font of the texts</param>
    /// <param name="buttonModel">gives the look of the "Équiper" button</param>
    public EquipTooltip(RectTransform parent, Text textModel, Image buttonModel, Color buttonTextColor)
    {
        this.parent = parent;
        this.textModel = textModel;
        this.buttonModel = buttonModel;
        this.buttonTextColor = buttonTextColor;
    }

    public bool IsShownFor(Item item)
    {
        return root != null && shown == item;
    }

    public void Hide()
    {
        if (root != null) Object.Destroy(root);
        root = null;
        shown = null;
    }

    /// <summary>Shows what "item" changes when it replaces "current" (null: the slot is empty), next to its cell.</summary>
    public void Show(Item item, Item current, RectTransform cell, System.Action onEquip)
    {
        Hide();
        LoadIcons();
        List<Equipment.Change> changes = Equipment.Compare(current != null ? current.stats : null, item.stats);

        float width = parent.rect.width * 0.72f;
        float line = parent.rect.width * 0.06f;
        float pad = line * 0.4f;
        float titleHeight = line * 1.1f;
        float subtitleHeight = line * 0.8f;
        float buttonHeight = line * 1.5f;
        float height = pad * 3 + titleHeight + subtitleHeight + buttonHeight;
        foreach (Equipment.Change change in changes) height += RowHeight(change, line);
        if (changes.Count == 0) height += line;

        RectTransform rect = UIBuilder.Child("EquipTooltip", parent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(width, height));
        rect.localPosition = Place(cell, width, height, pad);
        root = rect.gameObject;
        shown = item;

        UIBuilder.Image(rect, null, backgroundColor, true);
        Outline outline = root.AddComponent<Outline>();
        outline.effectColor = Item.getRarityColor(item.rarity);
        outline.effectDistance = new Vector2(3, -3);
        // a click outside of the button closes the tooltip
        root.AddComponent<Button>().onClick.AddListener(Hide);

        float y = pad;
        Text title = UIBuilder.Text(Row(rect, "Title", ref y, titleHeight, pad, width, height),
            textModel, item.name, FontSize(titleHeight), TextAnchor.MiddleLeft);
        title.color = Color.white;
        Text subtitle = UIBuilder.Text(Row(rect, "Subtitle", ref y, subtitleHeight, pad, width, height),
            textModel, current != null ? "à la place de : " + current.name : "emplacement vide", FontSize(subtitleHeight), TextAnchor.MiddleLeft);
        subtitle.color = secondaryColor;

        float iconSize = line * 0.6f;
        foreach (Equipment.Change change in changes)
        {
            float rowHeight = RowHeight(change, line);
            Color color = change.IsGood ? goodColor : badColor;
            RectTransform icon = UIBuilder.Child("Icon", rect, new Vector2(0f, 1f),
                new Vector2(pad + iconSize / 2, -(y + rowHeight / 2)), new Vector2(iconSize, iconSize));
            UIBuilder.Image(icon, Icon(change.kind), color, false).preserveAspect = true;
            Text text = UIBuilder.Text(Row(rect, "Change", ref y, rowHeight, pad + iconSize + pad, width, height),
                textModel, change.text, FontSize(line), TextAnchor.MiddleLeft);
            text.color = color;
        }
        if (changes.Count == 0)
        {
            Text none = UIBuilder.Text(Row(rect, "NoChange", ref y, line, pad, width, height),
                textModel, "Aucun changement", FontSize(line), TextAnchor.MiddleLeft);
            none.color = secondaryColor;
        }

        y += pad;
        RectTransform buttonRect = UIBuilder.Child("EquipButton", rect,
            new Vector2(0.2f, 1 - (y + buttonHeight) / height), new Vector2(0.8f, 1 - y / height));
        Image buttonImage = UIBuilder.Image(buttonRect, buttonModel.sprite, buttonModel.color, true);
        buttonImage.type = buttonModel.type;
        buttonRect.gameObject.AddComponent<Button>().onClick.AddListener(() => onEquip());
        Text label = UIBuilder.Text(UIBuilder.Child("Text", buttonRect, new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.85f)),
            textModel, "Équiper", FontSize(buttonHeight), TextAnchor.MiddleCenter);
        label.color = buttonTextColor;
    }

    // the description of a skill is a sentence: it needs two lines
    private static float RowHeight(Equipment.Change change, float line)
    {
        return change.IsSkill ? line * 1.8f : line;
    }

    private static int FontSize(float rowHeight)
    {
        return Mathf.Max(10, Mathf.FloorToInt(rowHeight * 0.7f));
    }

    /// <summary>A row over the width of the tooltip, "y" below its top; moves y to the bottom of the row.</summary>
    private static RectTransform Row(RectTransform rect, string name, ref float y, float rowHeight, float left, float width, float height)
    {
        RectTransform row = UIBuilder.Child(name, rect,
            new Vector2(left / width, 1 - (y + rowHeight) / height), new Vector2(0.97f, 1 - y / height));
        y += rowHeight;
        return row;
    }

    /// <summary>
    /// The center of the tooltip in the parent: below the cell of the item, above it when there is no room
    /// below, and always inside the parent.
    /// </summary>
    private Vector3 Place(RectTransform cell, float width, float height, float margin)
    {
        Vector3[] corners = new Vector3[4];
        cell.GetWorldCorners(corners);
        Vector2 min = parent.InverseTransformPoint(corners[0]);
        Vector2 max = parent.InverseTransformPoint(corners[2]);
        Rect area = parent.rect;

        float x = (min.x + max.x) / 2;
        float y = min.y - margin - height / 2;
        if (y - height / 2 < area.yMin + margin) y = max.y + margin + height / 2;

        float xLimit = Mathf.Max(0, area.width / 2 - width / 2 - margin);
        float yLimit = Mathf.Max(0, area.height / 2 - height / 2 - margin);
        x = Mathf.Clamp(x, area.center.x - xLimit, area.center.x + xLimit);
        y = Mathf.Clamp(y, area.center.y - yLimit, area.center.y + yLimit);
        return new Vector3(x, y, 0);
    }

    private static Sprite Icon(Equipment.ChangeKind kind)
    {
        switch (kind)
        {
            case Equipment.ChangeKind.StatUp: return chevronUp;
            case Equipment.ChangeKind.StatDown: return chevronDown;
            case Equipment.ChangeKind.SkillGained: return check;
            default: return cross;
        }
    }

    private static void LoadIcons()
    {
        if (chevronUp != null) return;
        chevronUp = UIBuilder.PixelSprite(
            ".......",
            "...#...",
            "..###..",
            ".##.##.",
            "##...##",
            "#.....#",
            ".......");
        chevronDown = UIBuilder.PixelSprite(
            ".......",
            "#.....#",
            "##...##",
            ".##.##.",
            "..###..",
            "...#...",
            ".......");
        check = UIBuilder.PixelSprite(
            ".......",
            "......#",
            ".....##",
            "#...##.",
            "##.##..",
            ".###...",
            "..#....");
        cross = UIBuilder.PixelSprite(
            "#.....#",
            "##...##",
            ".##.##.",
            "..###..",
            ".##.##.",
            "##...##",
            "#.....#");
    }
}
