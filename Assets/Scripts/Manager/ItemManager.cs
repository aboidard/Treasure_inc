using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The sprites of the items, by the "graphics" number the server gives to each item: those of the sprite sheet,
/// set in the scene, then the skins of equipment drawn by code (ItemArt).
/// </summary>
public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    public List<Sprite> sprites;
    void Awake()
    {
        if (sprites.Count == ItemArt.FirstId)
        {
            sprites.AddRange(ItemArt.Sprites);
        }
        else
        {
            Debug.LogError("ItemManager has " + sprites.Count + " sprites instead of " + ItemArt.FirstId
                + ": the skins of ItemArt would not get the ids the server knows");
        }
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance d'ItemManager dans la scène");
            return;
        }
        instance = this;
    }

    public Sprite PickSprite(int id)
    {
        // a sprite this version of the client does not have yet: any sprite is better than an error
        if (id < 0 || id >= sprites.Count) return sprites[0];
        return sprites[id];
    }
}
