using UnityEngine;

public class BackgroundDatabase : MonoBehaviour
{
    public GameObject[] mine;
    public static BackgroundDatabase instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    public GameObject PickBackground(Biome type)
    {
        switch (type)
        {
            case Biome.MINE:
                return mine[Random.Range(0, mine.Length)];
            default:
                break;
        }
        return null;

    }
}
public enum Biome
{
    GARDEN,
    MINE,
    LAC,
    INFERNO
}
