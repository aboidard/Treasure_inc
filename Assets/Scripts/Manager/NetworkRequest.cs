public class NetworkRequest
{
    public const int GET_USER_ITEMS = 1;
    public const int ADD_USER_ITEMS = 2;
    public const int REMOVE_USER_ITEMS = 3;
    public const int SEND_EXPEDITION = 4;
    public int request;
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
}
