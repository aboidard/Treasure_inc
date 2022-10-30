using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsWindow;
    public GameObject creditsWindow;
    public Text infos;

    private void Start()
    {
        NetworkManager.Instance.AddRequest(new NetworkRequest(NetworkRequest.VERSION_SERVER));
    }

    public void StartGame()
    {
        NetworkManager.Instance.AddRequest(new NetworkRequest(NetworkRequest.LOGIN, new string[]{"true"}));
    }

    public void Settings()
    {
        settingsWindow.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsWindow.SetActive(false);
    }

    public void Credit()
    {
        creditsWindow.SetActive(true);
    }

    public void CloseCredit()
    {
        creditsWindow.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void OnEnable()
    {
        NetworkManager.OnLoadInfos += MajInfosUI;
    }
    private void OnDisabled()
    {
        NetworkManager.OnLoadInfos -= MajInfosUI;
    }

    void MajInfosUI()
    {
        infos.text = "Identifiant : " + NetworkManager.Instance.publicKey
        + "\nVersion : " + Application.version;
        if (null != NetworkManager.Instance.serverVersion)
        {
            infos.text += "\nServeur : " + NetworkManager.Instance.serverVersion;
        }
    }
}
