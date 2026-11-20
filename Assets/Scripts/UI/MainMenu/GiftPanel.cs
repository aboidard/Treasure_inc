using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The gift box: the gifts sent by the administrators (the launch of the game, an event...) or given by the game.
/// Each gift shows what it is for at its left and its treasure chest at its right: the chest gives the reward
/// (gems, money, items) when it is clicked. A gift may expire.
/// The scene has no such panel: it is built by code with the frame of the crew panel (see Create), and opened by
/// the GiftButton next to the button of the quests.
/// </summary>
public class GiftPanel : Panel
{
    public static GiftPanel instance;

    private const float rowHeight = 230f;
    private const float rowGap = 12f;
    private const float timerPeriod = 1f;
    private static readonly Color rowColor = new Color(0f, 0f, 0f, 0.3f);
    private static readonly Color rewardColor = new Color(1f, 0.88f, 0.45f);
    private static readonly Color expiryColor = new Color(1f, 0.6f, 0.4f);

    private Text subtitle;
    private RectTransform viewport;
    private RectTransform content;
    private Text empty;
    // the chests of the gifts: they beat
    private readonly List<RectTransform> chests = new List<RectTransform>();
    // the expiry of each gift shown, refreshed as time goes by
    private readonly List<KeyValuePair<Text, GiftAPI>> expiries = new List<KeyValuePair<Text, GiftAPI>>();
    private float nextTimerUpdate;
    private GiftManager manager;

    /// <summary>Creates the panel next to the crew panel, with its place, its size and its frame, and its button.</summary>
    public static GiftPanel Create(GameObject crewPanel)
    {
        RectTransform source = crewPanel.GetComponent<RectTransform>();
        GameObject created = new GameObject("GiftPanel", typeof(RectTransform));
        RectTransform rect = created.GetComponent<RectTransform>();
        rect.SetParent(source.parent, false);
        rect.anchorMin = source.anchorMin;
        rect.anchorMax = source.anchorMax;
        rect.pivot = source.pivot;
        rect.sizeDelta = source.sizeDelta;
        // like the other panels, it waits out of the screen until it is opened
        rect.anchoredPosition = source.anchoredPosition;
        rect.SetSiblingIndex(source.GetSiblingIndex() + 1);

        Image frame = crewPanel.GetComponent<Image>();
        Image image = UIBuilder.Image(rect, frame.sprite, frame.color, true);
        image.type = frame.type;

        GiftPanel gifts = created.AddComponent<GiftPanel>();
        gifts.panel = created;
        // the panels close each other through this list
        List<Panel> panels = UIManager.instance.panels.ToList();
        panels.Add(gifts);
        UIManager.instance.panels = panels.ToArray();

        GiftButton.Create(crewPanel.GetComponentInParent<Canvas>().rootCanvas);
        return gifts;
    }

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    protected override void WillStart()
    {
        Transform root = panel.transform;
        Text model = UIBuilder.ModelText;

        UIBuilder.Text(UIBuilder.Child("Title", root, new Vector2(0.06f, 0.925f), new Vector2(0.94f, 0.985f)),
            model, "Cadeaux", 73, TextAnchor.MiddleCenter);
        subtitle = UIBuilder.Text(UIBuilder.Child("Subtitle", root, new Vector2(0.05f, 0.875f), new Vector2(0.95f, 0.92f)),
            model, "", 40, TextAnchor.MiddleCenter);

        // the transparent image takes the drags that scroll the list
        viewport = UIBuilder.Child("Viewport", root, new Vector2(0.04f, 0.09f), new Vector2(0.96f, 0.865f));
        UIBuilder.Image(viewport, null, new Color(0f, 0f, 0f, 0f), true);
        viewport.gameObject.AddComponent<RectMask2D>();
        // as wide as the viewport, as high as its gifts, hanging from the top
        content = UIBuilder.Child("Content", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
        content.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = rowGap;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.viewport = viewport;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;

        empty = UIBuilder.Text(UIBuilder.Child("Empty", root, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f)),
            model, "", 44, TextAnchor.MiddleCenter);

        UIBuilder.Text(UIBuilder.Child("Hint", root, new Vector2(0.05f, 0.025f), new Vector2(0.95f, 0.075f)),
            model, "Récompenses, événements, surprises de l'équipe : tout arrive ici !", 30, TextAnchor.MiddleCenter);

        manager = GiftManager.Instance;
        manager.Changed += OnGiftsChanged;
    }

    private void OnDestroy()
    {
        // not through Instance, which would create a manager while the game is closing
        if (manager != null) manager.Changed -= OnGiftsChanged;
    }

    private void OnGiftsChanged()
    {
        if (IsOpen()) Render();
    }

    protected override void WillShow()
    {
        Render();
        GiftManager.Instance.Refresh();
    }

    void Update()
    {
        float beat = 1f + 0.07f * Mathf.Sin(Time.unscaledTime * 5f);
        foreach (RectTransform chest in chests)
        {
            if (chest != null) chest.localScale = Vector3.one * beat;
        }
        if (Time.unscaledTime >= nextTimerUpdate)
        {
            nextTimerUpdate = Time.unscaledTime + timerPeriod;
            foreach (KeyValuePair<Text, GiftAPI> expiry in expiries)
            {
                if (expiry.Key != null) expiry.Key.text = Gift.Expiry(expiry.Value) ?? "";
            }
        }
    }

    private void Render()
    {
        chests.Clear();
        expiries.Clear();
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            // destroyed at the end of the frame: hidden until then, so the layout forgets it at once
            content.GetChild(i).gameObject.SetActive(false);
            Destroy(content.GetChild(i).gameObject);
        }

        GiftsAPI gifts = GiftManager.Instance.gifts;
        int count = gifts != null ? gifts.count : 0;
        subtitle.text = gifts == null ? ""
            : count == 0 ? "Aucun cadeau en attente"
            : count == 1 ? "Un cadeau vous attend !"
            : count + " cadeaux vous attendent !";
        empty.text = gifts == null ? "Chargement..." : count == 0 ? "Votre boîte est vide... pour l'instant !" : "";
        if (gifts == null) return;

        foreach (GiftAPI gift in gifts.gifts)
        {
            RectTransform row = UIBuilder.Child("Gift" + gift.id, content, Vector2.zero, Vector2.one);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = rowHeight;
            FillRow(row, gift);
        }
        // back to the top
        content.anchoredPosition = Vector2.zero;
    }

    /// <summary>A gift: what it is for and what it gives at its left, its chest at its right.</summary>
    private void FillRow(RectTransform row, GiftAPI gift)
    {
        UIBuilder.Image(row, null, rowColor, false);
        Text model = UIBuilder.ModelText;

        Text title = UIBuilder.Text(UIBuilder.Child("Title", row, new Vector2(0.03f, 0.5f), new Vector2(0.75f, 0.95f)),
            model, gift.title, 40, TextAnchor.MiddleLeft);
        title.color = Color.white;

        Text reward = UIBuilder.Text(UIBuilder.Child("Reward", row, new Vector2(0.03f, 0.25f), new Vector2(0.75f, 0.5f)),
            model, Gift.Reward(gift.reward), 32, TextAnchor.MiddleLeft);
        reward.color = gift.reward != null && gift.reward.gems > 0 ? Gems.TextColor : rewardColor;

        Text expiry = UIBuilder.Text(UIBuilder.Child("Expiry", row, new Vector2(0.03f, 0.04f), new Vector2(0.75f, 0.25f)),
            model, Gift.Expiry(gift) ?? "", 26, TextAnchor.MiddleLeft);
        expiry.color = expiryColor;
        expiries.Add(new KeyValuePair<Text, GiftAPI>(expiry, gift));

        // the whole corner of the row takes the click on the treasure chest
        RectTransform claim = UIBuilder.Child("Claim", row, new Vector2(0.77f, 0.06f), new Vector2(0.985f, 0.94f));
        Image hit = UIBuilder.Image(claim, null, new Color(0f, 0f, 0f, 0f), true);
        RectTransform chest = UIBuilder.Child("Chest", claim, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(144f, 108f));
        UIBuilder.Image(chest, QuestPanel.ChestBody, QuestPanel.chestColor, false);
        UIBuilder.Image(UIBuilder.Child("Detail", chest, Vector2.zero, Vector2.one),
            QuestPanel.ChestDetail, QuestPanel.chestDetailColor, false);
        Button button = claim.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => Claim(gift));
        chests.Add(chest);
    }

    private void Claim(GiftAPI gift)
    {
        if (GiftManager.Instance.isBusy) return;
        SoundManager.Play(Sfx.ChestOpen);
        GiftManager.Instance.Claim(gift, (claimed, items) =>
        {
            if (claimed == null) return;
            SoundManager.Play(Sfx.Coins);
            int money = claimed.reward != null ? claimed.reward.money : 0;
            int gems = claimed.reward != null ? claimed.reward.gems : 0;
            string gains = (gems > 0 ? "  +" + Gems.Amount(gems) : "")
                + (money > 0 ? "  +" + UIBuilder.Number(money) + " pièces" : "");
            LootPanel.instance.ShowItems("Cadeau !" + gains, items);
        });
    }
}
