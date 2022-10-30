using System;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _Instance;
    public string publicKey;
    public string privateKey;
    static readonly HttpClient client = new HttpClient();
    private string apiUrl;
    public bool logedIn = false;
    public bool loggingProcess = false;
    private Queue queue = new Queue();
    public String serverVersion;

    //api end point
    private string createUserEndPoint = "/user/create";
    private string loginEndPoint = "/login/{0}";
    private string versionEndPoint = "/version";
    private string getUserItemsEndPoint = "/user/{0}/items"; 
    private string addUserItemsEndPoint = "/user/{0}/items";
    private string removeUserItemsEndPoint = "/user/{0}/sellitems";
    private string sendExpeditionEndPoint = "/user/{0}/expedition";
    //event
    public delegate void LoadInfosAction();
    public static event LoadInfosAction OnLoadInfos;


	public static NetworkManager Instance
	{
		get
		{
			if (!_Instance)
			{
				// NOTE: read docs to see directory requirements for Resources.Load!
				var prefab = Resources.Load<GameObject>("Prefabs/System/NetworkManager");
				// create the prefab in your scene
				var inScene = Instantiate<GameObject>(prefab);
				// try find the instance inside the prefab
				_Instance = inScene.GetComponentInChildren<NetworkManager>();
				// guess there isn't one, add one
				if (!_Instance) _Instance = inScene.AddComponent<NetworkManager>();
				// mark root as DontDestroyOnLoad();
				DontDestroyOnLoad(_Instance.transform.root.gameObject);
			}
			return _Instance;
		}
	}

    private void Start()
    {
#if UNITY_EDITOR
        apiUrl = "http://127.0.0.1:8081";
        Debug.Log("url de l'api (dev) : " + apiUrl);
#else
        apiUrl = "https://api.treasure-inc.fr";
        Debug.Log("url de l'api (prod) : " + apiUrl);
#endif

        LoadAndSaveData.instance.LoadUserKeys();
    }

    public void Update()
    {

        if (this.queue.Count > 0)
        {
            NetworkRequest request = (NetworkRequest)this.queue.Dequeue();
            Debug.Log("request " + request.request);

            if (request.request == NetworkRequest.LOGIN || 
                request.request == NetworkRequest.VERSION_SERVER)
            {
                OpenRequest(request);
            }
            if(logedIn)
            {
                SecureRequest(request);
            }

        }
    }

    private void OpenRequest(NetworkRequest request)
    {
        switch (request.request)
        {
            case NetworkRequest.VERSION_SERVER:
                Debug.Log("request versionServer");
                StartCoroutine(GetVersionServer());
                break;
                
            case NetworkRequest.LOGIN:
                Debug.Log("request LOGIN");
                if(logedIn) break;
                    StartCoroutine(Login(Convert.ToBoolean(request.parameters[0])));
                break;
            default:
                break;
        }
    }

    private void SecureRequest(NetworkRequest request)
    {
        switch (request.request)
        {
            case NetworkRequest.GET_USER_ITEMS:
                Debug.Log("request GetUserItems");
                StartCoroutine(GetUserItems(false));
                break;

            case NetworkRequest.ADD_USER_ITEMS:
                Debug.Log("request RemoveUserItems");
                StartCoroutine(AddUserItems(request.parameters[0]));
                break;

            case NetworkRequest.REMOVE_USER_ITEMS:
                Debug.Log("request RemoveUserItems");
                StartCoroutine(RemoveUserItems(request.parameters[0]));
                break;

            case NetworkRequest.SEND_EXPEDITION:
                Debug.Log("request SendExpedition");
                StartCoroutine(SendExpedition(request.parameters[0]));
                break;

            default:
                break;
        }
    }

    public void AddRequest(NetworkRequest request)
    {
        Debug.Log("add request " + request + " | " + this.queue);
        this.queue.Enqueue(request);
    }

    IEnumerator GetVersionServer()
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl + versionEndPoint))
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
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                ServerInfos infos = JsonConvert.DeserializeObject<ServerInfos>(webRequest.downloadHandler.text);
                serverVersion = infos.version;
            }
            OnLoadInfos();
            Loader.instance.SetLoading(false);
        }
        
    }

    IEnumerator Login(bool changeScene)
    {
        if(loggingProcess) yield break;
        
        bool createUser;
        string apiEndPoint;

        loggingProcess = true;
        Loader.instance.SetLoading(true);

        if (publicKey == "" || privateKey == ""){
            createUser = true;
            apiEndPoint = createUserEndPoint;
        }
        else
        {
            createUser = false;
            apiEndPoint = loginEndPoint;
        }
        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(apiUrl + apiEndPoint, this.publicKey)))
        {
            if(!createUser) 
                webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
            yield return webRequest.SendWebRequest();

            try{
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    throw new LoginException(webRequest.error);
                }
                else
                {
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    User user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                    Debug.Log("user : " + user);
                    if(createUser){
                        NetworkManager.Instance.publicKey = user.publicKey.ToString();
                        NetworkManager.Instance.privateKey = user.privateKey.ToString();
                        LoadAndSaveData.instance.SaveUserKeys();
                        client.DefaultRequestHeaders.Add("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
                    }
                    Inventory.Instance.CurrentMoney = user.money;
                    logedIn = true;
                    StartCoroutine(GetUserItems(changeScene));
                }
            }catch(LoginException e)
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

    }

    IEnumerator GetUserItems(bool changeScene)
    {
        Loader.instance.SetLoading(true);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(apiUrl + getUserItemsEndPoint, this.publicKey)))
        {
            webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
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
                if(changeScene)
                    SceneManager.LoadScene("MainScene");
            }
        }
    }

    IEnumerator AddUserItems(string items)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl +addUserItemsEndPoint, this.publicKey), items))
        {
            //TODO: remove this hack and use post before
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                List<ItemAPI> itemsFromJSON = JsonConvert.DeserializeObject<List<ItemAPI>>(webRequest.downloadHandler.text);
                foreach (ItemAPI it in itemsFromJSON)
                {
                    Item myItem = Item.CreateItemAPI(it);
                    Inventory.Instance.AddItem(myItem);
                }
                Loader.instance.SetLoading(false);
            }
        }
    }
    IEnumerator RemoveUserItems(string items)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + removeUserItemsEndPoint, this.publicKey), items))
        {
            //TODO: remove this hack and use post before
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                Loader.instance.SetLoading(false);
            }
        }
    }

    IEnumerator SendExpedition(string expedition)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + sendExpeditionEndPoint, this.publicKey), expedition))
        {
            //TODO: remove this hack and use post before
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
            }
            Loader.instance.SetLoading(false);
        }
    }
    private void OnEnable()
    {
        NetworkManager.OnLoadInfos += MajInfosUI;
    }
    private void OnDisabled()
    {
        NetworkManager.OnLoadInfos -= MajInfosUI;
    }

    private void MajInfosUI()
    {

    }
}