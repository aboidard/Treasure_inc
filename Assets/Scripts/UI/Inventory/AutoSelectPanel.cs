using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class AutoSelectPanel : MonoBehaviour
{
    public GameObject autoSelectPanel;
    public static AutoSelectPanel instance;
    public Toggle common;
    public Toggle uncommon;
    public Toggle rare;
    public Toggle epic;
    public Toggle legendary;

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

    public void ClickOk()
    {
        List<Rarity> listRarity = new List<Rarity>();
        if(common.isOn) listRarity.Add(Rarity.Common);
        if(uncommon.isOn) listRarity.Add(Rarity.Uncommon);
        if(rare.isOn) listRarity.Add(Rarity.Rare);
        if(epic.isOn) listRarity.Add(Rarity.Epic);
        if(legendary.isOn) listRarity.Add(Rarity.Legendary);
        InventoryPanel.instance.AutoSelect(listRarity);
        ClosePanel();
    }
    public void ShowAutoSelect()
    {
        OpenPanel();
    }

    public void OpenPanel()
    {
        autoSelectPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        autoSelectPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        autoSelectPanel.SetActive(false);
    }
}
