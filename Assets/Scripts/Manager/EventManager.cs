using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance d'EventManager dans la scène");
            return;
        }
        instance = this;
    }
    public void GenerateEvent(Expedition expedition)
    {
        expedition.items.Add(Item.GenerateRandomItem());
    }


}
