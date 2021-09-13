using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public string publicKey = "12345678900";
    public string privateKey = "private_key_test";

    static readonly HttpClient client = new HttpClient();

    private string apiUrl;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    public async void Start()
    {
#if UNITY_EDITOR
        apiUrl = "http://127.0.0.1:8081";
        Debug.Log("url de l'api (dev) : " + apiUrl);
#else
        apiUrl = "http://api.treasure-inc.fr";
        Debug.Log("url de l'api (prod) : " + apiUrl);
#endif
        await this.GetVersionServer();
        await this.GetUserItems();
    }

    public async Task GetVersionServer()
    {
        string responseBody = "";
        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl + "/version");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Debug.Log(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught!");
            Debug.Log("Message : " + e.Message);
        }

    }

    public async Task GetUserItems()
    {
        string responseBody = "";
        try
        {
            HttpResponseMessage response = await client.GetAsync(String.Format(apiUrl + "/user/{0}/items", this.publicKey));
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            Inventory.instance.InitItemsFromJSON(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught!");
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

            Inventory.instance.AddItemsFromJSON(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.Log("\nException Caught!");
            Debug.Log("Message : " + e.Message);
        }

    }
}
