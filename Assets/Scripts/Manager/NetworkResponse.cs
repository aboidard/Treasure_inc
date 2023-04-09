using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkResponse
{
    public int id;

    public NetworkResponse(int id)
    {
        this.id = id;
    }
}
