using System.Net.Http;
using System.Threading.Tasks;

using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance; 
    
    static readonly HttpClient client = new HttpClient();

    void Awake()
    {
        if(instance != null){
            Debug.LogWarning("plus d'une instance de NetworkManager dans la scène");
            return;
        }
        instance = this;
    }

    public async void Start()
    {
        await this.GetVersionServer();
    }

    public async Task GetVersionServer()
    {
        string responseBody = "";
        try	
        {
            HttpResponseMessage response = await client.GetAsync("http://127.0.0.1:8081/version");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);

            Debug.Log(responseBody);
        }
        catch(HttpRequestException e)
        {
            Debug.Log("\nException Caught!");	
            Debug.Log("Message : " + e.Message);
        }

    }
}
