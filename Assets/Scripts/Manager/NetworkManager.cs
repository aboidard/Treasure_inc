using System;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public string publicKey;
    public string privateKey;
    static readonly HttpClient client = new HttpClient();
    private string apiUrl;
    public bool logedIn = false;
    private Queue queue = new Queue();
    public Text serverInfos;


    private void Awake()
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
#if UNITY_EDITOR
        apiUrl = "http://127.0.0.1:8081";
        Debug.Log("url de l'api (dev) : " + apiUrl);
#else
        apiUrl = "https://api.treasure-inc.fr";
        Debug.Log("url de l'api (prod) : " + apiUrl);
#endif

        LoadAndSaveData.instance.LoadUserKeys();


        StartCoroutine(GetHealthCheckServer());

        if (publicKey == "" || privateKey == "")
        {
            StartCoroutine(GetNewUserAndLogin());
        }
        else
        {
            StartCoroutine(Login());
        }
    }

    public void Update()
    {
        if (!logedIn)
        {
            return;
        }
        if (this.queue.Count > 0)
        {
            NetworkRequest request = (NetworkRequest)this.queue.Dequeue();
            Debug.Log("request " + request.request);
            switch (request.request)
            {
                case NetworkRequest.GET_USER_ITEMS:
                    Debug.Log("request GetUserItems");
                    StartCoroutine(GetUserItems());
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
    }

    public void AddRequest(NetworkRequest request)
    {
        Debug.Log("add request " + request + " | " + this.queue);
        this.queue.Enqueue(request);
    }

    IEnumerator GetHealthCheckServer()
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl + "/healthcheck"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
                serverInfos.text = webRequest.error;
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                ServerInfos infos = JsonConvert.DeserializeObject<ServerInfos>(webRequest.downloadHandler.text);
                serverInfos.text = "Version serveur : " + infos.version;
            }
            Loader.instance.SetLoading(false);
        }
    }

    IEnumerator GetNewUserAndLogin()
    {
        Loader.instance.SetLoading(true);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl + "/user/create"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                User user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                NetworkManager.instance.publicKey = user.publicKey.ToString();
                NetworkManager.instance.privateKey = user.privateKey.ToString();
                Inventory.instance.CurrentMoney = user.money;
                LoadAndSaveData.instance.SaveUserKeys();
                client.DefaultRequestHeaders.Add("X-PRIVATE-KEY", NetworkManager.instance.privateKey);
                logedIn = true;
            }
            Loader.instance.SetLoading(false);
        }
    }

    IEnumerator Login()
    {
        Loader.instance.SetLoading(true);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(apiUrl + "/login/{0}", this.publicKey)))
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
                User user = JsonConvert.DeserializeObject<User>(webRequest.downloadHandler.text);
                Debug.Log("user : " + user);
                Inventory.instance.CurrentMoney = user.money;
                logedIn = true;
                Loader.instance.SetLoading(false);
            }
        }
    }

    IEnumerator GetUserItems()
    {
        Loader.instance.SetLoading(true);

        using (UnityWebRequest webRequest = UnityWebRequest.Get(String.Format(apiUrl + "/user/{0}/items", this.publicKey)))
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
                    Inventory.instance.AddItem(myItem);
                }

                Loader.instance.SetLoading(false);
            }
        }
    }

    IEnumerator AddUserItems(string items)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + "/user/{0}/items", this.publicKey), items))
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
                    Inventory.instance.AddItem(myItem);
                }
                Loader.instance.SetLoading(false);
            }
        }
    }
    IEnumerator RemoveUserItems(string items)
    {
        Loader.instance.SetLoading(true);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + "/user/{0}/sellitems", this.publicKey), items))
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
        using (UnityWebRequest webRequest = UnityWebRequest.Put(String.Format(apiUrl + "/user/{0}/expedition", this.publicKey), expedition))
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
}