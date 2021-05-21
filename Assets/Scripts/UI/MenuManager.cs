using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public GameObject actionPanel;
    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de MenuManager dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        ClosePanel();    
    }
    public void OnClickAction()
    {
        actionPanel.SetActive(!actionPanel.activeInHierarchy);
    } 
    public void ClosePanel()
    {
        this.actionPanel.SetActive(false);
    }
}
