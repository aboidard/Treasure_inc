using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Text DebugInfo;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        Inventory.instance.UpdateMoneyUI();
    }

    public void FixedUpdate()
    {
        DebugInfo.text = "Time.fixedDeltaTime : " + Time.fixedDeltaTime;
        DebugInfo.text += "\nTime.deltaTime : " + Time.deltaTime;
        DebugInfo.text += "\nfps : " + (1f / Time.unscaledDeltaTime);

        Inputs();
    }

    private void Inputs()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Item item = Item.GenerateScriptableItem(1);
            Inventory.instance.AddItem(item);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Location location = LocationsDatabase.instance.allLocations.Single(x => x.id == 1);
            ExpeditionManager.instance.SendExpedition(location, 10);
        }
    }
}
