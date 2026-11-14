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
    //public static string apiUrl = "https://api.treasure-inc.fr";
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
    public static string digCaseEndPoint = "/V1/user/{0}/expedition/dig";

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

    /// <summary>
    /// Sends the body as JSON to an end point of the current user, then calls onDone with the HTTP status and the
    /// response text; the status is 0 and the text is the error when the API could not be reached.
    /// </summary>
    public IEnumerator PostJson(string endPoint, object body, Action<long, string> onDone)
    {
        string url = String.Format(apiUrl + endPoint, publicKey);
        using (UnityWebRequest webRequest = UnityWebRequest.Put(url, JsonConvert.SerializeObject(body)))
        {
            //TODO: remove this hack and use post before
            webRequest.method = "POST";
            webRequest.SetRequestHeader("X-PRIVATE-KEY", privateKey);
            webRequest.SetRequestHeader("Content-Type", "application/json");
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError("ConnectionException: " + webRequest.error);
                onDone(0, webRequest.error);
            }
            else
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                onDone(webRequest.responseCode, webRequest.downloadHandler.text);
            }
        }
    }
}