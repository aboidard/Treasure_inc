using UnityEngine;

public class ItemsDatabase : MonoBehaviour
{
    public Item[] allItems;
    public static ItemsDatabase instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }
}
