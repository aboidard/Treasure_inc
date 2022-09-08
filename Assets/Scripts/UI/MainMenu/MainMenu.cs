using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsWindow;
    public GameObject creditsWindow;
    public Text infos;

    public void StartGame()
    {
        NetworkManager.instance.AddRequest(new NetworkRequest(NetworkRequest.LOGIN,new string[]{"true"}));
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
        infos.text = "Identifiant : " + NetworkManager.instance.publicKey
        + "\nVersion : " + Application.version;
        if (null != NetworkManager.instance.serverVersion)
        {
            infos.text += "\nServeur : " + NetworkManager.instance.serverVersion;
        }
    }
}
