using UnityEngine;
using UnityEngine.UI;
public class SettingsPanel : Panel
{
    public Text idUtilisateur;
    public static SettingsPanel instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    protected override void WillShow()
    {
        idUtilisateur.text = NetworkManager.instance.publicKey;
    }
}
