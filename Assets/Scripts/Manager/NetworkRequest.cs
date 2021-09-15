public class NetworkRequest
{
    public const int GET_USER_ITEMS = 1;
    public const int ADD_USER_ITEMS = 2;
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
