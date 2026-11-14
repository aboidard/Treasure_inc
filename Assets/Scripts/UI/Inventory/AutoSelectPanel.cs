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
    /// <summary>Whether the equipable items are selected too; created below the rarities when the scene has none.</summary>
    public Toggle equipable;

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
        if (equipable == null) equipable = CreateEquipableToggle();
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
        InventoryPanel.instance.AutoSelect(listRarity, equipable == null || equipable.isOn);
        ClosePanel();
    }
    /// <summary>A copy of the last rarity toggle, one row below it, unchecked: the equipment is kept by default.</summary>
    private Toggle CreateEquipableToggle()
    {
        Toggle toggle = Instantiate(legendary, legendary.transform.parent);
        toggle.name = "EquipableToggle";
        RectTransform rect = toggle.GetComponent<RectTransform>();
        RectTransform last = legendary.GetComponent<RectTransform>();
        RectTransform previous = epic.GetComponent<RectTransform>();
        rect.anchoredPosition = last.anchoredPosition + (last.anchoredPosition - previous.anchoredPosition);
        toggle.isOn = false;
        Text label = toggle.GetComponentInChildren<Text>();
        if (label != null) label.text = "équipables";
        return toggle;
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
