using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LootPanel : MonoBehaviour
{
    public GameObject lootPanel;
    public Text lootTitle;
    public Transform listObject;
    public GameObject objectPanelPrefab;
    public static LootPanel instance;
    private List<Item> items;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        ClosePanel();
    }

    public void ShowLoot(List<Item> items, string title)
    {
        OpenPanel();
        StartCoroutine(AddItem(title, items.Count));

    }

    IEnumerator AddItem(String title, int nb)
    {

        Loader.instance.SetLoading(true);
        var apiEndPoint = NetworkManager.addUserItemsEndPoint;
        using (UnityWebRequest webRequest = UnityWebRequest.Post(String.Format(NetworkManager.apiUrl + apiEndPoint, NetworkManager.Instance.publicKey), "{\"nb\":" + nb + "}"))
        {
            webRequest.SetRequestHeader("X-PRIVATE-KEY", NetworkManager.Instance.privateKey);
            Debug.Log("SendWebRequest : " + webRequest);
            yield return webRequest.SendWebRequest();
            Debug.Log("done SendWebRequest");
            try
            {
                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("LoginException: " + webRequest.error);
                    throw new LoginException(webRequest.error);
                }
                else
                {
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    lootTitle.text = title;
                    this.items = JsonConvert.DeserializeObject<List<Item>>(webRequest.downloadHandler.text);

                    //init
                    for (int i = 0; i < listObject.childCount; i++)
                    {
                        Destroy(listObject.GetChild(i).gameObject);
                    }

                    for (int i = 0; i < this.items.Count; i++)
                    {
                        GameObject panel = Instantiate(objectPanelPrefab, listObject);
                        ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
                        objectPanel.itemName.text = this.items[i].name;
                        objectPanel.itemImage.sprite = this.items[i].graphics;
                        objectPanel.itemBorder.color = Item.getRarityColor(this.items[i].rarity);

                        objectPanel.item = this.items[i];
                    }
                }
            }
            catch (LoginException e)
            {
                Debug.Log("Error: " + e.Message);
                MessagePanel.instance.DisplayMessage("Erreur !", "Impossible de se connecter, veuillez réessayer ultérieurement !\n" + e.Message);
            }
            finally
            {
                Loader.instance.SetLoading(false);
            }
        }
    }

    public void OpenPanel()
    {

        lootPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        lootPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
}
