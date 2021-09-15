using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    void Awake()
    {
        GameObject objs = GameObject.Find("NetworkManager");

        // if (objs.Length > 1)
        // {
        //     Destroy(this.gameObject);
        // }

        DontDestroyOnLoad(this.gameObject);
    }
}