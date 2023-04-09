using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;
using Google.Play.AppUpdate;
using Google.Play.Common;
using Newtonsoft.Json;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsWindow;
    public GameObject creditsWindow;
    public Text infos;
    private AppUpdateManager appUpdateManager;
    private bool loggingProcess = false;

    private void Start()
    {
        //appUpdateManager = new AppUpdateManager();
        //StartCoroutine(CheckForUpdate());
        //NetworkManager.Instance.AddRequest(new NetworkRequest(NetworkRequest.VERSION_SERVER));
        StartCoroutine(GetServerVersion());
    }
    IEnumerator GetServerVersion()
    {
        Loader.instance.SetLoading(true);
        LoadAndSaveData.instance.LoadUserKeys();
        using (UnityWebRequest webRequest = UnityWebRequest.Get(NetworkManager.apiUrl + NetworkManager.versionEndPoint))
        {
            webRequest.timeout = 5;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de se connecter, veuillez réessayer ultérieurement !\n" + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                ServerInfos infos = JsonConvert.DeserializeObject<ServerInfos>(webRequest.downloadHandler.text);
                NetworkManager.serverVersion = infos.version;
            }
            MajInfosUI();
            Loader.instance.SetLoading(false);
        }
    }

    public void StartGame()
    {
        StartCoroutine(Login());

    }

    IEnumerator Login()
    {
        if (loggingProcess) yield break;

        bool createUser;
        string apiEndPoint;

        loggingProcess = true;
        Loader.instance.SetLoading(true);

        if (NetworkManager.Instance.publicKey == "" || NetworkManager.Instance.privateKey == "")
        {
            createUser = true;
            apiEndPoint = NetworkManager.createUserEndPoint;
        }
        else
        {
            createUser = false;
            apiEndPoint = NetworkManager.loginEndPoint;
        }
        Debug.Log("apiEndPoint: " + apiEndPoint);
        Debug.Log("createUser: " + createUser);
        Debug.Log("request: " + String.Format(NetworkManager.apiUrl + apiEndPoint, NetworkManager.Instance.publicKey));
        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(NetworkManager.apiUrl + apiEndPoint, NetworkManager.Instance.publicKey)))
        {
            if (!createUser)
                webRequest.SetRequestHeader("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);

            Debug.Log("SendWebRequest : " + webRequest);
            yield return webRequest.SendWebRequest();
            Debug.Log("done SendWebRequest");

            try
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("LoginException: " + webRequest.error);
                    throw new LoginException(webRequest.error);
                }
                else
                {
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    User user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                    Debug.Log("user : " + user);
                    if (createUser)
                    {
                        NetworkManager.Instance.publicKey = user.publicKey.ToString();
                        NetworkManager.Instance.privateKey = user.privateKey.ToString();
                        LoadAndSaveData.instance.SaveUserKeys();
                        NetworkManager.client.DefaultRequestHeaders.Add("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
                    }
                    Inventory.Instance.CurrentMoney = user.money;
                    NetworkManager.logedIn = true;
                }
            }
            catch (LoginException e)
            {
                Debug.Log("Error: " + e.Message);
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de se connecter, veuillez réessayer ultérieurement !\n" + e.Message);
            }
            finally
            {
                loggingProcess = false;
                Loader.instance.SetLoading(false);
            }
        }
        if (NetworkManager.logedIn)
        {
            yield return GetUserItems();
        }
    }

    IEnumerator GetUserItems()
    {
        Loader.instance.SetLoading(true);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(NetworkManager.apiUrl + NetworkManager.getUserItemsEndPoint, NetworkManager.Instance.publicKey)))
        {
            webRequest.SetRequestHeader("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);

                List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(webRequest.downloadHandler.text);
                foreach (ItemAPI it in itemsFromJSON ?? Enumerable.Empty<ItemAPI>())
                {
                    Item myItem = Item.CreateItemAPI(it);
                    Inventory.Instance.AddItem(myItem);
                }

                Loader.instance.SetLoading(false);
                SceneManager.LoadScene("MainScene");
            }
        }
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

    void MajInfosUI()
    {
        infos.text = "Identifiant : " + NetworkManager.Instance.publicKey
        + "\nVersion : " + Application.version;
        if (null != NetworkManager.serverVersion)
        {
            infos.text += "\nServeur : " + NetworkManager.serverVersion;
        }
    }

    private IEnumerator CheckForUpdate()
    {
        PlayAsyncOperation<AppUpdateInfo, AppUpdateErrorCode> appUpdateInfoOperation = appUpdateManager.GetAppUpdateInfo();

        yield return appUpdateInfoOperation;

        if (appUpdateInfoOperation.IsSuccessful)
        {
            var appUpdateInfoResult = appUpdateInfoOperation.GetResult();
            Debug.Log(appUpdateInfoResult.UpdateAvailability.ToString());

            if (appUpdateInfoResult.UpdateAvailability == UpdateAvailability.UpdateAvailable)
            {
                Debug.Log(UpdateAvailability.UpdateAvailable.ToString());
            }
            else
            {
                Debug.Log("version app OK");
            }

            var appUpdateOptions = AppUpdateOptions.ImmediateAppUpdateOptions();
            StartCoroutine(StartImmediateUpdate(appUpdateInfoResult, appUpdateOptions));
        }
    }

    IEnumerator StartImmediateUpdate(AppUpdateInfo appUpdateInfoOp_i, AppUpdateOptions appUpdateOptions_i)
    {
        var startUpdateRequest = appUpdateManager.StartUpdate(appUpdateInfoOp_i, appUpdateOptions_i);
        yield return startUpdateRequest;
    }

}
