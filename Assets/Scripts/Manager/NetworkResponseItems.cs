using System.Collections.Generic;

public class NetworkResponseItems : NetworkResponse
{
    List<Item> items;

    public NetworkResponseItems(int id, List<Item> items) : base(id)
    {
        this.items = items;
    }

    public List<Item> getItems()
    {
        return items;
    }
}
