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
    public Event GenerateRandomEvent(Event[] targetAvailableList, GameObject targetViewport)
    {
        Event target = Instantiate(targetAvailableList[0], targetViewport.transform);
        target.SetReward(Item.GenerateRandomItems(1));
        return target;
    }
}
