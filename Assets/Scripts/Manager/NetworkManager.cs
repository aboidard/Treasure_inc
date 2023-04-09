using System;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager _Instance;
    public string publicKey;
    public string privateKey;
    public static readonly HttpClient client = new HttpClient();

#if UNITY_EDITOR
    public static string apiUrl = "http://127.0.0.1:8081";
#else
    public static string apiUrl = "https://api.treasure-inc.fr";
#endif



    public static bool logedIn = false;
    private Queue<NetworkRequest> queueRequest = new Queue<NetworkRequest>();
    public static String serverVersion;

    //api end point
    public static string createUserEndPoint = "/V1/user/create";
    public static string loginEndPoint = "/V1/login/{0}";
    public static string versionEndPoint = "/version";
    public static string getUserItemsEndPoint = "/V1/user/{0}/items";
    public static string addUserItemsEndPoint = "/V1/user/{0}/items";
    public static string removeUserItemsEndPoint = "/V1/user/{0}/sellitems";
    public static string sendExpeditionEndPoint = "/V1/user/{0}/expedition";

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
        LoadAndSaveData.instance.LoadUserKeys();
    }

    public void Update()
    {

        /*if (this.queueRequest.Count > 0)
        {
            NetworkRequest request = (NetworkRequest)this.queueRequest.Dequeue();
            Debug.Log("request " + request.request);

            if (request.request == NetworkRequest.LOGIN ||
                request.request == NetworkRequest.VERSION_SERVER)
            {
                OpenRequest(request);
            }
            if (logedIn)
            {
                SecureRequest(request);
            }

        }*/
    }

    private void OpenRequest(NetworkRequest request)
    {
        switch (request.request)
        {
            case NetworkRequest.VERSION_SERVER:
                Debug.Log("request versionServer");
                //StartCoroutine(GetVersionServer());
                break;

            case NetworkRequest.LOGIN:
                Debug.Log("request LOGIN, params : " + request.parameters[0]);
                if (logedIn) break;
                //StartCoroutine(Login(Convert.ToBoolean(request.parameters[0])));
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
                //StartCoroutine(GetUserItems(false));
                break;

            case NetworkRequest.ADD_USER_ITEMS:
                Debug.Log("request RemoveUserItems");
                StartCoroutine(AddUserItems(request, request.parameters[0]));
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
        Debug.Log("add request " + request + " | " + this.queueRequest);
        this.queueRequest.Enqueue(request);
    }



    IEnumerator AddUserItems(NetworkRequest request, string items)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + addUserItemsEndPoint, this.publicKey), items))
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
                List<Item> itemsToAdd = new List<Item>();
                foreach (ItemAPI it in itemsFromJSON)
                {
                    Item myItem = Item.CreateItemAPI(it);
                    Inventory.Instance.AddItem(myItem);
                    itemsToAdd.Append(myItem);
                }
                //queueResponse.Add(new NetworkResponseItems(request.id, itemsToAdd));
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
        //TODO : add a callback to the request
        //queueDone.Enqueue(request);
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

    private void MajInfosUI()
    {

    }
}