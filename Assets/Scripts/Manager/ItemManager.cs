using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    public List<Sprite> sprites;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance d'ItemManager dans la scène");
            return;
        }
        instance = this;
    }

    public (Sprite graphics, int graphicsId) PickOneRandomSprite()
    {
        int randNumber = Random.Range(0, sprites.Count);
        return (sprites[randNumber], randNumber);
    }

    public Sprite PickSprite(int id)
    {
        return sprites[id];
    }
}
