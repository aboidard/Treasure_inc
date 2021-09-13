using UnityEngine;

public class UIManager : MonoBehaviour
{

    public Panel[] panels;

    public static UIManager instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    public void CloseAllPanel()
    {
        foreach (var panel in panels)
        {
            panel.Close();
        }
    }
}
