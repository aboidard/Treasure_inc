using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        StartCoroutine(CheckServer(true));
    }

    /// <summary>
    /// Reads the healthcheck of the API: server version, maintenance and oldest client version allowed.
    /// The maintenance (or the update to make) is shown at once when showBlock; a maintenance that stopped the game
    /// while playing is shown in any case.
    /// </summary>
    IEnumerator CheckServer(bool showBlock)
    {
        Loader.instance.SetLoading(true);
        LoadAndSaveData.instance.LoadUserKeys();
        using (UnityWebRequest webRequest = UnityWebRequest.Get(NetworkManager.apiUrl + NetworkManager.healthcheckEndPoint))
        {
            // the API waits 2 s at most for the engine
            webRequest.timeout = 10;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError
                || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + webRequest.error);
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de se connecter, veuillez réessayer ultérieurement !\n" + webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                try
                {
                    ServerInfos infos = ServerInfos.Parse(webRequest.downloadHandler.text);
                    NetworkManager.serverInfos = infos;
                    NetworkManager.serverVersion = infos.version;
                }
                catch (JsonException e)
                {
                    Debug.LogError("Unreadable healthcheck: " + e.Message);
                }
            }
        }
        // the maintenance that stopped the game is over when the server says so
        if (NetworkManager.serverInfos != null && !NetworkManager.serverInfos.InMaintenance)
        {
            NetworkManager.pendingMaintenance = null;
        }
        MajInfosUI();
        Loader.instance.SetLoading(false);
        if (showBlock || NetworkManager.pendingMaintenance != null)
        {
            ShowBlock();
        }
    }

    /// <summary>The maintenance under way, or null: the one told by the healthcheck, or the one that stopped the game.</summary>
    MaintenanceInfos CurrentMaintenance()
    {
        if (NetworkManager.serverInfos != null && NetworkManager.serverInfos.InMaintenance) return NetworkManager.serverInfos.maintenance;
        return NetworkManager.pendingMaintenance;
    }

    /// <summary>Tells why the player cannot log in; false when nothing keeps them from it.</summary>
    bool ShowBlock()
    {
        MaintenanceInfos maintenance = CurrentMaintenance();
        if (maintenance != null)
        {
            MessagePanel.instance.DisplayMessage("Maintenance", maintenance.Text);
            return true;
        }
        if (NetworkManager.serverInfos != null && NetworkManager.serverInfos.UpdateRequired)
        {
            MessagePanel.instance.DisplayMessage("Mise à jour requise",
                "Une nouvelle version de Treasure Inc. est disponible : mettez l'application à jour pour continuer à jouer."
                + "\n\nVotre version : " + Application.version
                + "\nVersion minimale : " + NetworkManager.serverInfos.minClientVersion);
            return true;
        }
        return false;
    }

    public void StartGame()
    {
        StartCoroutine(CheckThenLogin());
    }

    /// <summary>Reads the healthcheck again (a maintenance may have started or ended), then logs in if nothing blocks.</summary>
    IEnumerator CheckThenLogin()
    {
        if (loggingProcess) yield break;
        yield return CheckServer(false);
        if (ShowBlock())
        {
            if (CurrentMaintenance() == null && Application.platform == RuntimePlatform.Android)
            {
                // the update: the page of the game in the Play Store
                Application.OpenURL("market://details?id=" + Application.identifier);
            }
            yield break;
        }
        yield return Login();
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
                else if (MaintenanceError.Read(webRequest.responseCode, webRequest.downloadHandler.text) is MaintenanceInfos maintenance)
                {
                    // started since the healthcheck
                    NetworkManager.pendingMaintenance = maintenance;
                    MajInfosUI();
                    MessagePanel.instance.DisplayMessage("Maintenance", maintenance.Text);
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("LoginException: HTTP " + webRequest.responseCode + " " + webRequest.downloadHandler.text);
                    throw new LoginException("HTTP " + webRequest.responseCode);
                }
                else
                {
                    Debug.Log("Received: " + webRequest.downloadHandler.data);

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
                    Inventory.Instance.CurrentGems = user.gems;
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
                // a login after a maintenance: the items of the previous one are still there
                Inventory.Instance.ClearItems();
                foreach (ItemAPI it in itemsFromJSON ?? Enumerable.Empty<ItemAPI>())
                {
                    Item myItem = Item.CreateItemAPI(it);
                    Inventory.Instance.AddItem(myItem);
                }

                Loader.instance.SetLoading(false);
                SoundManager.Play(Sfx.GameStart);
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
        // stays on screen when the message is closed
        MaintenanceInfos maintenance = CurrentMaintenance();
        if (maintenance != null)
        {
            string reason = string.IsNullOrEmpty(maintenance.reason) ? "en cours" : maintenance.reason;
            if (reason.Length > 120) reason = reason.Substring(0, 117) + "...";
            infos.text += "\n\nMaintenance : " + reason;
            DateTime? end = maintenance.LocalEnd;
            if (end.HasValue)
            {
                infos.text += "\nFin prévue le " + end.Value.ToString("dd/MM/yyyy 'à' HH:mm", System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
            }
        }
        else if (NetworkManager.serverInfos != null && NetworkManager.serverInfos.UpdateRequired)
        {
            infos.text += "\n\nMise à jour requise (version minimale " + NetworkManager.serverInfos.minClientVersion + ")";
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
