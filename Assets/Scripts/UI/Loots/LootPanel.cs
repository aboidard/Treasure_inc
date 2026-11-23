using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

public class LootPanel : MonoBehaviour
{
    public GameObject lootPanel;
    public Text lootTitle;
    public Transform listObject;
    public GameObject objectPanelPrefab;
    public static LootPanel instance;
    private List<Item> items;
    // the items then the experience, which scroll together when they are taller than the panel
    private RectTransform content;
    private XpGainView xpView;

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

    public void ShowLoot(string expeditionId, string title)
    {
        lootTitle.text = title;
        InitPanel();
        OpenPanel();
        StartCoroutine(OpenChest(expeditionId));
    }

    /// <summary>Shows items the user just received otherwise than from a chest: the reward of a quest.</summary>
    public void ShowItems(string title, List<Item> received)
    {
        lootTitle.text = title;
        InitPanel();
        OpenPanel();
        this.items = received ?? new List<Item>();
        FillPanel();
        bool rare = this.items.Exists(item => item.rarity == Rarity.Epic || item.rarity == Rarity.Legendary);
        SoundManager.Play(rare ? Sfx.RareItem : Sfx.ItemsFound);
    }

    IEnumerator OpenChest(string expeditionId)
    {
        Loader.instance.SetLoading(true);
        long status = 0;
        string response = null;
        yield return NetworkManager.Instance.PostJson(NetworkManager.addUserItemsEndPoint,
            new OpenChestRequest { expeditionId = expeditionId }, (code, text) =>
            {
                status = code;
                response = text;
            });
        Loader.instance.SetLoading(false);

        if (status != 201)
        {
            // the server knows whether the character of the expedition is back; if it is not, the chest can be
            // opened again later
            CharacterManager.Instance.Refresh(ok => { if (ok) ExpeditionManager.instance.RecoverLostExpeditions(); });
            MessagePanel.instance.DisplayMessage("Erreur !", "Impossible d'ouvrir le coffre, veuillez réessayer ultérieurement !\n" + response);
            yield break;
        }

        // the chest is opened: the character of the expedition is available again
        CharacterManager.Instance.MarkFree(expeditionId);
        // an opened chest counts for the quests
        QuestManager.Instance.Refresh();

        // an API older than the levels answers the items alone, as a list
        OpenChestResponse chest = response.TrimStart().StartsWith("[")
            ? new OpenChestResponse { items = JsonConvert.DeserializeObject<List<ItemAPI>>(response) }
            : JsonConvert.DeserializeObject<OpenChestResponse>(response);
        this.items = new List<Item>();
        foreach (ItemAPI it in chest.items ?? new List<ItemAPI>())
        {
            Item myItem = Item.CreateItemAPI(it);
            this.items.Add(myItem);

            //add the item to the inventory
            Inventory.Instance.AddItem(myItem);
        }
        if (chest.floors > 0 && chest.floorsCompleted < chest.floors)
        {
            // the character died on the way: it only brings back what it found before
            GameCharacter fallen = chest.xp != null ? CharacterManager.Instance.Get(chest.xp.character) : null;
            lootTitle.text = (fallen != null ? fallen.name : "Votre personnage") + " est tombé à l'étage "
                + (chest.floorsCompleted + 1) + "/" + chest.floors
                + (this.items.Count == 0 ? " et revient les mains vides..." : " : voici ce qu'il a pu rapporter.");
        }
        // the chest of an event also gives gems or money
        if (chest.balance != null)
        {
            Inventory.Instance.CurrentMoney = chest.balance.money;
            Inventory.Instance.CurrentGems = chest.balance.gems;
        }
        if (chest.gems > 0 || chest.money > 0)
        {
            List<string> won = new List<string>();
            if (chest.gems > 0) won.Add("<color=#" + ColorUtility.ToHtmlStringRGB(Gems.TextColor) + ">+ " + Gems.Amount(chest.gems) + "</color>");
            if (chest.money > 0) won.Add("+ " + UIBuilder.Number(chest.money) + " pièces");
            lootTitle.text += "\n" + string.Join("   ", won);
        }
        FillPanel();
        ShowXp(chest.xp);
        if (TeamPanel.instance != null) TeamPanel.instance.RenderIfOpen();
        bool rare = this.items.Exists(item => item.rarity == Rarity.Epic || item.rarity == Rarity.Legendary);
        SoundManager.Play(rare ? Sfx.RareItem : Sfx.ItemsFound);
    }

    public void OpenPanel()
    {
        lootPanel.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        lootPanel.SetActive(true);
    }

    public void InitPanel()
    {
        MakeScrollable();
        for (int i = 0; i < listObject.childCount; i++)
        {
            Destroy(listObject.GetChild(i).gameObject);
        }
        if (xpView != null) Destroy(xpView.gameObject);
        xpView = null;
        content.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// The list of the scene only holds the items, and as many as the panel is tall. It is moved, once, into a
    /// content that scrolls in its place: the experience of the character comes below the items, whatever their
    /// number.
    /// </summary>
    private void MakeScrollable()
    {
        if (content != null) return;

        RectTransform list = listObject.GetComponent<RectTransform>();
        RectTransform viewport = UIBuilder.Child("Scroll", list.parent, list.anchorMin, list.anchorMax);
        viewport.offsetMin = list.offsetMin;
        viewport.offsetMax = list.offsetMax;
        viewport.SetSiblingIndex(list.GetSiblingIndex());
        // invisible, but it takes the drags made between the items
        UIBuilder.Image(viewport, null, new Color(0f, 0f, 0f, 0f), true);
        viewport.gameObject.AddComponent<RectMask2D>();

        content = UIBuilder.Child("Content", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 20f;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        list.SetParent(content, false);

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
    }

    /// <summary>After the items: what the expedition brought to its character. Nothing when nobody led it.</summary>
    private void ShowXp(XpGainAPI gain)
    {
        if (gain == null) return;

        // the view shows where the character comes from, so it is built before the character changes
        xpView = XpGainView.Create(content, lootTitle, gain, CharacterManager.Instance.Get(gain.character));
        CharacterManager.Instance.ApplyXp(gain);
    }
    public void FillPanel()
    {


        for (int i = 0; i < this.items.Count; i++)
        {
            GameObject panel = Instantiate(objectPanelPrefab, listObject);
            ObjectPanel objectPanel = panel.GetComponent<ObjectPanel>();
            if (objectPanel == null)
            {
                Debug.LogError("Object Panel prefab is missing ObjectPanel script.");
                return;
            }
            objectPanel.SetItem(this.items[i]);
        }
    }

    public void ClosePanel()
    {
        lootPanel.SetActive(false);
    }
}
