using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The gems, the premium currency: they pay the summons and are won with the daily bonus quest and the gifts.
/// Their icon is the red gem of the sheet of the items, the one the HUD shows next to their counter.
/// </summary>
public static class Gems
{
    /// <summary>The red gem in the sheet of the items (ItemManager.sprites): roguelikeitems_42.</summary>
    public const int SpriteId = 42;

    /// <summary>The red of the gem, for the texts that speak of gems.</summary>
    public static readonly Color TextColor = new Color(1f, 0.45f, 0.45f);

    public static Sprite Icon => ItemManager.instance != null ? ItemManager.instance.PickSprite(SpriteId) : null;

    /// <summary>The icon of a gem in a rectangle of its parent, keeping its shape.</summary>
    public static Image AddIcon(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        Image image = UIBuilder.Image(UIBuilder.Child("Gem", parent, anchorMin, anchorMax), Icon, Color.white, false);
        image.preserveAspect = true;
        return image;
    }

    /// <summary>"1 000 gemmes", "1 gemme".</summary>
    public static string Amount(int gems)
    {
        return UIBuilder.Number(gems) + (gems > 1 ? " gemmes" : " gemme");
    }
}
