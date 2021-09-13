using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
    public static MessageManager instance;
    public Text messageTitle;
    public Text messageText;
    public GameObject messagePanel;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    public void DisplayMessage(string title, string text)
    {
        this.messagePanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        this.messagePanel.SetActive(true);
        this.messageTitle.text = title;
        this.messageText.text = text;
    }

    public void CloseMessage()
    {
        this.messagePanel.SetActive(false);
    }
}
