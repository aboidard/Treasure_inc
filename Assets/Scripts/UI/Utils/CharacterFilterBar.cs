using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The row of buttons above a list of characters, built by code. From left to right:
///   the sort (a click moves to the next one), its direction, "favorites only", "available only" (where it makes
///   sense) and one button per rank, to hide or show the characters of this rank.
/// It changes the CharacterListOptions of its screen, then calls onChanged for the screen to show its list again.
/// </summary>
public class CharacterFilterBar : MonoBehaviour
{
    private const float gap = 0.01f;
    private static readonly string[] ranks = { "D", "C", "B", "A", "S", "SS", "SSS" };
    private static readonly Color offTint = new Color(1f, 1f, 1f, 0.35f);
    public static readonly Color FavoriteColor = new Color(1f, 0.8f, 0.15f);

    private static Sprite star;
    private static Sprite arrow;

    /// <summary>A star, white: the mark of a favorite character.</summary>
    public static Sprite Star
    {
        get
        {
            if (star == null)
            {
                star = UIBuilder.PixelSprite(
                    ".....#.....",
                    ".....#.....",
                    "....###....",
                    "....###....",
                    "###########",
                    ".#########.",
                    "..#######..",
                    "...#####...",
                    "..#######..",
                    "..###.###..",
                    ".##.....##.");
            }
            return star;
        }
    }

    private static Sprite Arrow
    {
        get
        {
            if (arrow == null)
            {
                arrow = UIBuilder.PixelSprite(
                    "...#...",
                    "...#...",
                    "...#...",
                    "#..#..#",
                    ".#.#.#.",
                    "..###..",
                    "...#...");
            }
            return arrow;
        }
    }

    private CharacterListOptions options;
    private Action onChanged;
    private Color buttonColor;
    private Text sortLabel;
    private RectTransform directionIcon;
    private Image favoritesButton;
    private Image favoritesIcon;
    private Image availableButton;
    private readonly Dictionary<string, Image> rankButtons = new Dictionary<string, Image>();
    private readonly Dictionary<string, Text> rankLabels = new Dictionary<string, Text>();

    /// <param name="rect">the place of the row: its buttons share its width</param>
    /// <param name="withAvailability">false for a list where every character is always available</param>
    /// <param name="onChanged">called after each change of the options</param>
    public static CharacterFilterBar Create(RectTransform rect, CharacterListOptions options, bool withAvailability,
        Action onChanged)
    {
        CharacterFilterBar bar = rect.gameObject.AddComponent<CharacterFilterBar>();
        bar.options = options;
        bar.onChanged = onChanged;
        bar.Build(withAvailability);
        bar.Refresh();
        return bar;
    }

    private void Build(bool withAvailability)
    {
        // the share of the width of each button
        List<float> weights = new List<float> { 0.26f, 0.07f, 0.07f };
        if (withAvailability) weights.Add(0.13f);
        float left = 1f - gap * (weights.Count + ranks.Length - 1);
        foreach (float weight in weights) left -= weight;
        for (int i = 0; i < ranks.Length; i++) weights.Add(left / ranks.Length);

        List<RectTransform> cells = new List<RectTransform>();
        float x = 0f;
        for (int i = 0; i < weights.Count; i++)
        {
            cells.Add(UIBuilder.Child("Cell" + i, transform, new Vector2(x, 0f), new Vector2(x + weights[i], 1f)));
            x += weights[i] + gap;
        }

        int cell = 0;
        Button sort = UIBuilder.Button(cells[cell++], "", 30, () => Change(options.NextSort));
        sortLabel = sort.GetComponentInChildren<Text>();
        buttonColor = sort.GetComponent<Image>().color;

        Button direction = UIBuilder.Button(cells[cell++], "", 30, () => Change(options.ToggleDirection));
        directionIcon = Icon(direction, Arrow).rectTransform;

        Button favorites = UIBuilder.Button(cells[cell++], "", 30, () => Change(options.ToggleFavoritesOnly));
        favoritesButton = favorites.GetComponent<Image>();
        favoritesIcon = Icon(favorites, Star);

        if (withAvailability)
        {
            Button available = UIBuilder.Button(cells[cell++], "Dispo", 30, () => Change(options.ToggleAvailableOnly));
            availableButton = available.GetComponent<Image>();
        }

        foreach (string rank in ranks)
        {
            string toggled = rank;
            Button button = UIBuilder.Button(cells[cell++], rank, 30, () => Change(() => options.ToggleRank(toggled)));
            Text label = button.GetComponentInChildren<Text>();
            Outline outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            rankButtons[rank] = button.GetComponent<Image>();
            rankLabels[rank] = label;
        }
    }

    private static Image Icon(Button button, Sprite sprite)
    {
        Image icon = UIBuilder.Image(UIBuilder.Child("Icon", button.transform, new Vector2(0.2f, 0.18f), new Vector2(0.8f, 0.82f)),
            sprite, UIBuilder.ButtonTextColor, false);
        icon.preserveAspect = true;
        return icon;
    }

    private void Change(Action change)
    {
        change();
        Refresh();
        onChanged?.Invoke();
    }

    /// <summary>Shows the options as they are: a filter that is off is dimmed, like a rank that is hidden.</summary>
    public void Refresh()
    {
        sortLabel.text = "Tri : " + options.SortName;
        // the arrow points down when the best come first
        directionIcon.localScale = new Vector3(1f, options.Descending ? 1f : -1f, 1f);

        favoritesButton.color = options.FavoritesOnly ? buttonColor : buttonColor * offTint;
        favoritesIcon.color = options.FavoritesOnly ? FavoriteColor : UIBuilder.ButtonTextColor;
        if (availableButton != null)
        {
            availableButton.color = options.AvailableOnly ? buttonColor : buttonColor * offTint;
        }
        foreach (string rank in ranks)
        {
            bool shown = options.ShowsRank(rank);
            rankButtons[rank].color = shown ? buttonColor : buttonColor * offTint;
            Color color = CharacterRank.Color(rank);
            rankLabels[rank].color = shown ? color : color * offTint;
        }
    }
}
