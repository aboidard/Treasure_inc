using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsWindow;
    public GameObject creditsWindow;

    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");
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

}
