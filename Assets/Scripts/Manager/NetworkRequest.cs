using UnityEngine;

public class NetworkRequest
{
    public const int GET_USER_ITEMS = 1;
    public const int ADD_USER_ITEMS = 2;
    public const int REMOVE_USER_ITEMS = 3;
    public const int SEND_EXPEDITION = 4;
    public const int LOGIN = 5;
    public const int VERSION_SERVER = 6;
    public int request;
    public int id = Random.Range(0, 1000000);
    public string[] parameters;

    public NetworkRequest(int request, string[] parameters)
    {
        this.request = request;
        this.parameters = parameters;
    }
    public NetworkRequest(int request)
    {
        this.request = request;
    }

    public int getId()
    {
        return id;
    }
}
