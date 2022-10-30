using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public static Loader instance;
    public bool loading = false;
    private GameObject loader;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
        this.loader = GameObject.FindGameObjectsWithTag("Loader")[0];
    }

    private void Start()
    {
        this.SetLoading(false);
    }

    public void SetLoading(bool value)
    {
        loading = value;
        loader.SetActive(loading);
    }
}
