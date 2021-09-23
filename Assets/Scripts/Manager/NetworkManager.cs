using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public string publicKey;
    public string privateKey;
    static readonly HttpClient client = new HttpClient();
    private string apiUrl;
    public bool logedIn = false;
    private Queue queue;


    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    async private void Start()
    {
        //initialisation de la queue FIFO
        queue = new Queue();
#if UNITY_EDITOR
        apiUrl = "http://127.0.0.1:8081";
        Debug.Log("url de l'api (dev) : " + apiUrl);
#else
        apiUrl = "https://api.treasure-inc.fr";
        Debug.Log("url de l'api (prod) : " + apiUrl);
#endif        
        LoadAndSaveData.instance.LoadUserKeys();
        if (publicKey == "" || privateKey == "")
        {
            await this.GetNewUserAndLogin();
        }
        else
        {
            await this.Login();
        }
        await this.GetHealthCheckServer();
    }

    public void Update()
    {
        if (!logedIn) return;
        if (this.queue.Count > 0)
        {
            NetworkRequest request = (NetworkRequest)this.queue.Dequeue();
            switch (request.request)
            {
                case NetworkRequest.GET_USER_ITEMS:
                    Task task = Task.Run(async () =>
                    {
                        await GetUserItems();
                    });
                    break;
                default:
                    break;
            }

        }
    }

    public void AddRequest(NetworkRequest request)
    {
        this.queue.Enqueue(request);
    }

    public async Task GetHealthCheckServer()
    {
        string responseBody = "";
        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl + "/healthcheck");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Debug.Log(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught! healthcheck");
            Debug.Log("Message : " + e.Message);
        }
    }

    public async Task GetNewUserAndLogin()
    {
        string responseBody = "";
        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl + "/user/create");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Debug.Log(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught! GetNewUserAndLogin");
            Debug.Log("Message : " + e.Message);
        }
        User user = JsonConvert.DeserializeObject<User>(responseBody);
        NetworkManager.instance.publicKey = user.publicKey.ToString();
        NetworkManager.instance.privateKey = user.privateKey.ToString();
        Inventory.instance.CurrentMoney = user.money;
        LoadAndSaveData.instance.SaveUserKeys();
        client.DefaultRequestHeaders.Add("X-PRIVATE-KEY", NetworkManager.instance.privateKey);
        logedIn = true;
    }

    public async Task Login()
    {
        string responseBody = "";
        client.DefaultRequestHeaders.Add("X-PRIVATE-KEY", privateKey);
        try
        {
            HttpResponseMessage response = await client.GetAsync(String.Format(apiUrl + "/login/{0}", this.publicKey));
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Debug.Log(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught! Login " + String.Format(apiUrl + "/login/{0}", this.publicKey));
            Debug.Log("Message : " + e.Message);
            return;
        }
        User user = JsonConvert.DeserializeObject<User>(responseBody);
        Debug.Log("user : " + user);
        Inventory.instance.CurrentMoney = user.money;
        logedIn = true;
    }

    public async Task GetUserItems()
    {
        string responseBody = "";
        try
        {
            Debug.Log("request items : " + String.Format(apiUrl + "/user/{0}/items", this.publicKey));
            HttpResponseMessage response = await client.GetAsync(String.Format(apiUrl + "/user/{0}/items", this.publicKey));
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Inventory.instance.initialItemLoaded = responseBody;

            //synchronisation avec Iventory
            Inventory.instance.loadingItems = false;
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught GetUserItems ! " + String.Format(apiUrl + "/user/{0}/items", this.publicKey));
            Debug.Log("Message : " + e.Message);
        }

    }

    public async Task AddUserItems(string items)
    {
        Debug.Log("list of the item to store : " + items);
        string responseBody = "";
        try
        {
            StringContent data = new StringContent(items, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(String.Format(apiUrl + "/user/{0}/items", this.publicKey), data);
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Inventory.instance.itemToAdd = responseBody;
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught!");
            Debug.Log("Message : " + e.Message);
        }

    }
}