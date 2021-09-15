using UnityEngine;

public class LoadAndSaveData : MonoBehaviour
{
    public static LoadAndSaveData instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    public void LoadUserKeys()
    {
        NetworkManager.instance.publicKey = PlayerPrefs.GetString("publicKey");
        NetworkManager.instance.privateKey = PlayerPrefs.GetString("privateKey");
    }

    public void SaveUserKeys()
    {
        PlayerPrefs.SetString("publicKey", NetworkManager.instance.publicKey);
        PlayerPrefs.SetString("privateKey", NetworkManager.instance.privateKey);
    }
}
