using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public static Loader instance;
    private bool loading = false;
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

    // Update is called once per frame
    void Update()
    {
        loader.SetActive(loading);
    }

    public void SetLoading(bool value)
    {
        this.loading = value;
    }
}
