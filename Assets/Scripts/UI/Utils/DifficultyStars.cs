using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The difficulty of a location or of an event as stars: 1 star is easy, 3 stars are hard. The stars it does not have
/// are drawn dim, so that the scale is always seen.
/// </summary>
public static class DifficultyStars
{
    public const int Max = 3;
    private static readonly Color off = new Color(0f, 0f, 0f, 0.35f);

    /// <summary>A row of Max stars filling the rectangle, from the left; returns the row to set with Show.</summary>
    public static RectTransform Create(Transform parent, Vector2 anchorMin, Vector2 anchorMax, int level)
    {
        RectTransform row = UIBuilder.Child("DifficultyStars", parent, anchorMin, anchorMax);
        for (int i = 0; i < Max; i++)
        {
            RectTransform cell = UIBuilder.Child("Star" + (i + 1), row, new Vector2(i / (float)Max, 0f), new Vector2((i + 1) / (float)Max, 1f));
            Image star = UIBuilder.Image(cell, CharacterFilterBar.Star, off, false);
            star.preserveAspect = true;
            Shadow shadow = cell.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
        Show(row, level);
        return row;
    }

    /// <summary>Lights the first level stars of a row made by Create.</summary>
    public static void Show(RectTransform row, int level)
    {
        int lit = Mathf.Clamp(level, 1, Max);
        for (int i = 0; i < row.childCount; i++)
        {
            row.GetChild(i).GetComponent<Image>().color = i < lit ? CharacterFilterBar.FavoriteColor : off;
        }
    }

    /// <summary>"Facile", "Moyen", "Difficile": what the stars mean, for the texts.</summary>
    public static string Label(int level)
    {
        if (level <= 1) return "Facile";
        if (level == 2) return "Moyen";
        return "Difficile";
    }
}
