using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel instance;
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
    private void Start()
    {
        this.messagePanel.SetActive(false);
    }

    /// <param name="sound">played with the message: most of them tell that something went wrong</param>
    public void DisplayMessage(string title, string text, Sfx sound = Sfx.Error)
    {
        SoundManager.Play(sound);
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
