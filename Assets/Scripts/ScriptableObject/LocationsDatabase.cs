using UnityEngine;

public class LocationsDatabase : MonoBehaviour
{
    public Location[] allLocations;
    public static LocationsDatabase instance;
    private void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de LocationsDatabase dans la scène");
            return;
        }
        instance = this;
    }
}
