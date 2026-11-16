using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPanel : Panel
{
    public GameObject listObject;
    private List<ObjectPanel> listObjectPanel;
    public GameObject objectPanelPrefab;
    public AutoSelectPanel autoSelectPanel;
    public static InventoryPanel instance;
    public Text title;
    public bool sellMode = false;
    public GameObject sellModeButton;
    public GameObject sellYesButton;
    public GameObject sellNoButton;
    public GameObject autoSelectButton;
    // set while the panel lists the items that fit a slot of a character (see ShowForEquip)
    private GameCharacter equipCharacter;
    private int equipSlot;
    private bool equipRequested;
    private GameObject unequipButton;
    private GameObject cancelButton;
    private EquipTooltip tooltip;
    private int listPaddingBottom = -1;

    public bool IsEquipMode => equipCharacter != null;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }
    /// <summary>
    /// Opens the inventory on the items that fit a slot of a character: a click on one of them shows what it
    /// changes compared to the item of the slot (see EquipTooltip), a second click or the button of the tooltip
    /// equips it. The item already in the slot can be taken off with the "Retirer" button, and the arrow at the
    /// bottom right goes back to the crew without changing anything.
    /// </summary>
    public void ShowForEquip(GameCharacter character, int slot)
    {
        equipCharacter = character;
        equipSlot = slot;
        equipRequested = true;
        if (IsOpen()) Close();
        Show();
    }

    /// <summary>True when the object panel is one of this list (the loots use the same prefab).</summary>
    public bool OwnsPanel(ObjectPanel objectPanel)
    {
        return listObjectPanel != null && listObjectPanel.Contains(objectPanel);
    }

    /// <summary>A click on an item of the list while a slot is being filled.</summary>
    public void SelectForEquip(ObjectPanel objectPanel)
    {
        if (!IsEquipMode || CharacterManager.Instance.isBusy) return;
        Item item = objectPanel.item;
        if (tooltip == null)
        {
            Text[] buttonTexts = autoSelectButton.GetComponentsInChildren<Text>(true);
            tooltip = new EquipTooltip(panel.GetComponent<RectTransform>(), title, autoSelectButton.GetComponent<Image>(),
                buttonTexts.Length > 0 ? buttonTexts[0].color : Color.black);
            // the tooltip is not in the list: it would stay where it is while its item moves
            ScrollRect scroll = listObject.GetComponentInParent<ScrollRect>();
            if (scroll != null) scroll.onValueChanged.AddListener(position => tooltip.Hide());
        }
        if (tooltip.IsShownFor(item))
        {
            EquipItem(item);
            return;
        }
        tooltip.Show(item, equipCharacter.Equipped(equipSlot), objectPanel.GetComponent<RectTransform>(), () => EquipItem(item));
    }

    /// <summary>Equips the item in the slot the panel was opened for, then goes back to the crew.</summary>
    public void EquipItem(Item item)
    {
        if (!IsEquipMode || CharacterManager.Instance.isBusy) return;
        CharacterManager.Instance.Equip(equipCharacter, equipSlot, item, ok =>
        {
            if (ok) SoundManager.Play(Sfx.Equip);
            BackToTeam(ok);
        });
    }

    public void UnequipItem()
    {
        if (!IsEquipMode || CharacterManager.Instance.isBusy) return;
        CharacterManager.Instance.Unequip(equipCharacter, equipSlot, ok =>
        {
            if (ok) SoundManager.Play(Sfx.Unequip);
            BackToTeam(ok);
        });
    }

    /// <summary>Goes back to the crew, leaving the slot as it is.</summary>
    public void CancelEquip()
    {
        if (!IsEquipMode || CharacterManager.Instance.isBusy) return;
        TeamPanel.instance.Show();
    }

    private void BackToTeam(bool ok)
    {
        // on an error the message is already shown, and the list stays as it is
        if (!ok || !IsOpen()) return;
        TeamPanel.instance.Show();
    }

    protected override void WillShow()
    {
        // opened from the menu, the panel is the plain inventory again
        if (!equipRequested)
        {
            equipCharacter = null;
            equipSlot = 0;
        }
        equipRequested = false;
        sellMode = false;

        sellModeButton.SetActive(!IsEquipMode);
        sellNoButton.SetActive(false);
        sellYesButton.SetActive(false);
        autoSelectButton.SetActive(false);
        if (tooltip != null) tooltip.Hide();
        //init
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            Destroy(listObject.transform.GetChild(i).gameObject);
        }

        listObjectPanel = new List<ObjectPanel>();
        for (int i = 0; i < Inventory.Instance.Items.Count; i++)
        {
            Item item = Inventory.Instance.Items[i];
            if (IsEquipMode && item.Slot != equipSlot) continue;

            GameObject panel = Instantiate(objectPanelPrefab, listObject.transform);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            listObjectPanel.Add(objectPanel);
            objectPanel.SetItem(item);
            if (IsEquipMode)
            {
                // the stats help to choose, as a click equips the item at once
                objectPanel.itemName.text = item.name + "\n" + Equipment.Summary(item.stats);
                objectPanel.itemName.resizeTextForBestFit = true;
                objectPanel.itemName.resizeTextMinSize = 10;
                objectPanel.itemName.resizeTextMaxSize = Mathf.Max(objectPanel.itemName.fontSize, 10);
            }
        }
        UpdateTitle();
        UpdateUnequipButton();
        UpdateCancelButton();
        float width = listObject.GetComponent<RectTransform>().rect.width;
        Vector2 newSize = new Vector2(width / 3, width / 3);
        listObject.GetComponent<GridLayoutGroup>().cellSize = newSize;

        // the choice of an equipment is a step of the crew screen: the camera stays on the crew
        if (!IsEquipMode) UIManager.instance.setCameraPosition(CameraMovement.POSITION_INVENTORY);
    }

    private void UpdateTitle()
    {
        if (!IsEquipMode)
        {
            title.text = sellMode ? "Vendre" : "Inventaire";
            return;
        }
        title.text = Equipment.SlotName(equipSlot) + (listObjectPanel.Count == 0 ? " : aucun objet" : " : choisir");
    }

    /// <summary>"Retirer" takes the place of the sell button while a slot holding an item is being filled.</summary>
    private void UpdateUnequipButton()
    {
        bool needed = IsEquipMode && equipCharacter.Equipped(equipSlot) != null;
        if (unequipButton == null)
        {
            if (!needed) return;
            // same look as the "auto select" button, with its two lines of text
            unequipButton = Instantiate(autoSelectButton, autoSelectButton.transform.parent);
            unequipButton.name = "UnequipButton";
            RectTransform rect = unequipButton.GetComponent<RectTransform>();
            RectTransform place = sellModeButton.GetComponent<RectTransform>();
            rect.anchorMin = place.anchorMin;
            rect.anchorMax = place.anchorMax;
            rect.anchoredPosition = place.anchoredPosition;
            rect.sizeDelta = place.sizeDelta;
            Text[] texts = unequipButton.GetComponentsInChildren<Text>(true);
            if (texts.Length > 0) texts[0].text = "Retirer";
            if (texts.Length > 1) texts[1].text = "l'objet";
            // the texts were sized for "Auto" and "Selection": a longer word was cut ("Reti")
            foreach (Text text in texts)
            {
                RectTransform textRect = text.rectTransform;
                textRect.sizeDelta = new Vector2(Mathf.Max(textRect.sizeDelta.x, rect.rect.width * 0.9f), textRect.sizeDelta.y);
                UIBuilder.FitOneLine(text);
            }
            Button button = unequipButton.GetComponent<Button>();
            // the copy must not open the auto select panel
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(UnequipItem);
        }
        unequipButton.SetActive(needed);
    }

    /// <summary>
    /// The arrow at the bottom right of the panel, shown while a slot is being filled: back to the crew without
    /// any change. The list keeps some room below its last row, so that the button never hides an item.
    /// </summary>
    private void UpdateCancelButton()
    {
        GridLayoutGroup grid = listObject.GetComponent<GridLayoutGroup>();
        if (listPaddingBottom < 0) listPaddingBottom = grid.padding.bottom;
        float side = sellModeButton.GetComponent<RectTransform>().rect.height * 1.4f;
        grid.padding.bottom = listPaddingBottom + (IsEquipMode ? Mathf.CeilToInt(side * 1.2f) : 0);

        if (cancelButton == null)
        {
            if (!IsEquipMode) return;
            Text[] texts = autoSelectButton.GetComponentsInChildren<Text>(true);
            Color iconColor = texts.Length > 0 ? texts[0].color : Color.black;
            // same look as the other buttons of the panel, with an arrow instead of the texts
            cancelButton = Instantiate(autoSelectButton, panel.transform);
            cancelButton.name = "CancelEquipButton";
            foreach (Text text in cancelButton.GetComponentsInChildren<Text>(true))
            {
                Destroy(text.gameObject);
            }
            RectTransform rect = cancelButton.GetComponent<RectTransform>();
            // bottom right, left of the scrollbar
            rect.anchorMin = rect.anchorMax = new Vector2(0.95f, 0.03f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(side, side);
            Sprite arrow = UIBuilder.PixelSprite(
                "..#......",
                ".##......",
                "#######..",
                ".##...##.",
                "..#....##",
                "......##.",
                "..#####..");
            RectTransform icon = UIBuilder.Child("Arrow", rect, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f));
            UIBuilder.Image(icon, arrow, iconColor, false).preserveAspect = true;
            Button button = cancelButton.GetComponent<Button>();
            // the copy must not open the auto select panel
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(CancelEquip);
        }
        cancelButton.transform.SetAsLastSibling();
        cancelButton.SetActive(IsEquipMode);
    }

    public void ToggleSellMode()
    {
        if (IsEquipMode) return;
        sellMode = !sellMode;
        UpdateTitle();
        sellModeButton.SetActive(!sellModeButton.activeInHierarchy);
        sellNoButton.SetActive(!sellModeButton.activeInHierarchy);
        sellYesButton.SetActive(!sellModeButton.activeInHierarchy);
        autoSelectButton.SetActive(!sellModeButton.activeInHierarchy);
        foreach (ObjectPanel item in listObjectPanel)
        {
            Toggle toggle = item.deleteToggle;
            toggle.isOn = false;
            toggle.gameObject.SetActive(!sellModeButton.activeInHierarchy);
        }
    }

    public void SellItems()
    {
        List<Item> listToSell = new List<Item>();
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            ObjectPanel objectPanel = listObject.transform.GetChild(i).GetComponent<ObjectPanel>();
            if (objectPanel.deleteToggle.isOn)
            {
                listToSell.Add(objectPanel.item);
            }
        }

        if (listToSell.Count == 0) return;
        var cost = 0;
        var nb = 0;
        Inventory.Instance.RemoveItemsAndPersist(listToSell);
        for (int i = 0; i < listToSell.Count; i++)
        {
            Inventory.Instance.AddMoney(listToSell[i].price);
            cost += listToSell[i].price;
            nb++;
        }
        if (nb > 0)
        {
            MessagePanel.instance.DisplayMessage("Vendu !", nb + " objets ont été vendu pour " + cost, Sfx.Coins);
            ToggleSellMode();
            Close();
        }
    }
    
    public void ShowAutoSelect()
    {
        autoSelectPanel.ShowAutoSelect();
    }
    /// <param name="includeEquipable">when false, the items that can be equipped are never selected</param>
    public void AutoSelect(List<Rarity> listRarityAutoSelect, bool includeEquipable = true)
    {
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            ObjectPanel objectPanel = listObject.transform.GetChild(i).GetComponent<ObjectPanel>();
            if (listRarityAutoSelect.Contains(objectPanel.item.rarity) && (includeEquipable || !objectPanel.item.IsEquipable))
            {
                objectPanel.deleteToggle.isOn = true;
            }
            else
            {
                objectPanel.deleteToggle.isOn = false;
            }
        }
    }
}
