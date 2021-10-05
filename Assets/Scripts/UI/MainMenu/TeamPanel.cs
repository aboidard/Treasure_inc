using UnityEngine;
public class TeamPanel : Panel
{
    public static TeamPanel instance;

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
        UIManager.instance.setCameraPosition(CameraMovement.POSITION_CREW);
    }

}
