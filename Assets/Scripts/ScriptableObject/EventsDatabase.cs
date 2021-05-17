using UnityEngine;

public class EventDatabase : MonoBehaviour
{
    public Event[] allEvents;
    public static EventDatabase instance;
    private void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de EventDatabase dans la scène");
            return;
        }
        instance = this;
    }
}
