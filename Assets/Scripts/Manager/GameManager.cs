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
        Inventory.Instance.UpdateMoneyUI();
        Inventory.Instance.UpdateGemsUI();
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
            Inventory.Instance.AddItem(item);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Location location = LocationsDatabase.instance.allLocations.Single(x => x.id == 1);
            GameCharacter character = CharacterManager.Instance.characters.FirstOrDefault(c => c.IsAvailable);
            if (character != null) ExpeditionManager.instance.SendExpedition(location, 10, null, character);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            //NetworkManager.Instance.AddRequest(new NetworkRequest(NetworkRequest.LOGIN, new string[]{"false"}));
        }
    }
}
