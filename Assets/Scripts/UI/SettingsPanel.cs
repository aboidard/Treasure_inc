using UnityEngine;
public class SettingsPanel : Panel
{
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
}
