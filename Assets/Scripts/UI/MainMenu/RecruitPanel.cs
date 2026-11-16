using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The recruitment: the user summons characters through a portal, by one draw or several at once, without
/// knowing who will come. The server makes the draws (see CharacterManager.Recruit) and SummonAnimation shows them.
/// The panel shows the portal, the chances of each rank and which of its characters the user already has; these
/// can be sorted and filtered like the other lists of characters (CharacterFilterBar).
/// The scene has no such panel: it is built by code with the frame of the crew panel (see Create).
/// </summary>
public class RecruitPanel : Panel
{
    public static RecruitPanel instance;

    private const int columns = 5;
    private static readonly Color unknownColor = new Color(0f, 0f, 0f, 0.6f);

    private Text portalName;
    private Text portalContent;
    private Text rates;
    private Text collectionTitle;
    private Text singleLabel;
    private Text multiLabel;
    private Button singleButton;
    private Button multiButton;
    private RectTransform collection;
    private RectTransform ringA;
    private RectTransform ringB;
    private RectTransform glow;
    private PortalAPI portal;
    private CharacterListOptions options;

    /// <summary>Creates the panel next to the crew panel, with its place, its size and its frame.</summary>
    public static RecruitPanel Create(GameObject crewPanel)
    {
        RectTransform source = crewPanel.GetComponent<RectTransform>();
        GameObject created = new GameObject("RecruitPanel", typeof(RectTransform));
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

        RecruitPanel recruit = created.AddComponent<RecruitPanel>();
        recruit.panel = created;
        // the panels close each other through this list
        List<Panel> panels = UIManager.instance.panels.ToList();
        panels.Add(recruit);
        UIManager.instance.panels = panels.ToArray();
        return recruit;
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
            model, "Recrutement", 73, TextAnchor.MiddleCenter);

        // the portal: two rings turning around a light
        RectTransform visual = UIBuilder.Child("Portal", root, new Vector2(0.25f, 0.765f), Vector2.zero, new Vector2(400f, 400f));
        Color portalColor = new Color(0.55f, 0.4f, 1f);
        glow = UIBuilder.Child("Glow", visual, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 400f));
        UIBuilder.Image(glow, FxSprites.Soft, new Color(portalColor.r, portalColor.g, portalColor.b, 0.75f), false);
        ringA = UIBuilder.Child("RingA", visual, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 380f));
        UIBuilder.Image(ringA, FxSprites.RingA, Color.white, false);
        ringB = UIBuilder.Child("RingB", visual, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280f, 280f));
        UIBuilder.Image(ringB, FxSprites.RingB, new Color(0.8f, 0.9f, 1f), false);
        UIBuilder.Image(UIBuilder.Child("Core", visual, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170f, 170f)),
            FxSprites.Soft, Color.white, false);

        portalName = UIBuilder.Text(UIBuilder.Child("PortalName", root, new Vector2(0.48f, 0.86f), new Vector2(0.95f, 0.915f)),
            model, "", 52, TextAnchor.MiddleLeft);
        portalContent = UIBuilder.Text(UIBuilder.Child("PortalContent", root, new Vector2(0.48f, 0.825f), new Vector2(0.95f, 0.86f)),
            model, "", 32, TextAnchor.MiddleLeft);
        rates = UIBuilder.Text(UIBuilder.Child("Rates", root, new Vector2(0.48f, 0.615f), new Vector2(0.95f, 0.82f)),
            model, "", 34, TextAnchor.UpperLeft);

        singleButton = UIBuilder.Button(UIBuilder.Child("Single", root, new Vector2(0.06f, 0.51f), new Vector2(0.48f, 0.595f)),
            "", 40, () => Draw(1));
        multiButton = UIBuilder.Button(UIBuilder.Child("Multi", root, new Vector2(0.52f, 0.51f), new Vector2(0.94f, 0.595f)),
            "", 40, () => Draw(portal != null ? portal.multi : 0));
        singleLabel = singleButton.GetComponentInChildren<Text>();
        multiLabel = multiButton.GetComponentInChildren<Text>();

        collectionTitle = UIBuilder.Text(UIBuilder.Child("CollectionTitle", root, new Vector2(0.06f, 0.465f), new Vector2(0.94f, 0.5f)),
            model, "", 34, TextAnchor.MiddleLeft);
        // the strongest first, until the user sorts them otherwise: what the portal promises
        options = CharacterListOptions.For("recruit", CharacterSort.Rank, true);
        CharacterFilterBar.Create(UIBuilder.Child("Filters", root, new Vector2(0.04f, 0.422f), new Vector2(0.96f, 0.458f)),
            options, false, Render);
        collection = UIBuilder.Child("Collection", root, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.414f));
    }

    void Update()
    {
        if (ringA == null) return;
        float time = Time.unscaledTime;
        ringA.localRotation = Quaternion.Euler(0f, 0f, time * 40f);
        ringB.localRotation = Quaternion.Euler(0f, 0f, -time * 65f);
        glow.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(time * 3f));
    }

    /// <summary>Opens the panel, unless it is already open.</summary>
    public void OpenRecruit()
    {
        if (!IsOpen()) Show();
    }

    protected override void WillShow()
    {
        UIManager.instance.setCameraPosition(CameraMovement.POSITION_CREW);
        Render();
        CharacterManager manager = CharacterManager.Instance;
        if (manager.portals.Count == 0)
        {
            manager.LoadPortals(ok =>
            {
                if (!IsOpen()) return;
                if (ok) Render();
                else portalName.text = "Portail injoignable";
            });
        }
        if (!manager.loaded)
        {
            manager.Refresh(ok => { if (ok && IsOpen()) Render(); });
        }
    }

    private void Render()
    {
        portal = CharacterManager.Instance.portals.FirstOrDefault();
        singleButton.interactable = portal != null;
        multiButton.interactable = portal != null;
        for (int i = collection.childCount - 1; i >= 0; i--)
        {
            Destroy(collection.GetChild(i).gameObject);
        }
        if (portal == null)
        {
            portalName.text = "Chargement...";
            portalContent.text = "";
            rates.text = "";
            collectionTitle.text = "";
            singleLabel.text = "Invoquer x1";
            multiLabel.text = "Invoquer x10";
            return;
        }

        List<CharacterAPI> content = portal.characters ?? new List<CharacterAPI>();
        portalName.text = portal.name;
        portalContent.text = content.Count + " personnages";
        rates.text = string.Join("\n", (portal.rates ?? new List<RankRateAPI>()).Select(rate =>
            CharacterRank.Colored(rate.rank) + "   " + rate.percent.ToString("0.##", CultureInfo.InvariantCulture).Replace(".", ",") + " %"));
        singleLabel.text = "Invoquer x1\n" + UIBuilder.Number(portal.cost);
        multiLabel.text = "Invoquer x" + portal.multi + "\n" + UIBuilder.Number(portal.cost * portal.multi);

        int owned = content.Count(c => CharacterManager.Instance.Get(c.num) != null);
        collectionTitle.text = "Collection : " + owned + " / " + content.Count;

        List<CharacterAPI> sorted = options.Apply(content, Describe);
        // the cells keep their size when the filters leave fewer characters
        int rows = Mathf.Max(1, Mathf.CeilToInt(content.Count / (float)columns));
        if (sorted.Count == 0 && content.Count > 0)
        {
            UIBuilder.Text(UIBuilder.Child("NoMatch", collection, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f)),
                UIBuilder.ModelText, "Aucun personnage ne correspond aux filtres.", 40, TextAnchor.MiddleCenter);
        }
        for (int i = 0; i < sorted.Count; i++)
        {
            int column = i % columns;
            int row = i / columns;
            RectTransform cell = UIBuilder.Child("Character" + sorted[i].num, collection,
                new Vector2(column / (float)columns, 1f - (row + 1) / (float)rows),
                new Vector2((column + 1) / (float)columns, 1f - row / (float)rows));
            FillCell(cell, sorted[i], CharacterManager.Instance.Get(sorted[i].num));
        }
    }

    /// <summary>A character of the portal for the sorts and the filters: as the user has it, or as the portal gives it.</summary>
    private static CharacterEntry Describe(CharacterAPI character)
    {
        GameCharacter owned = CharacterManager.Instance.Get(character.num);
        if (owned != null) return CharacterEntry.Of(owned);
        return new CharacterEntry
        {
            num = character.num,
            name = character.name,
            rank = character.rank,
            level = 0,
            power = Equipment.Power(character.stats),
            available = true,
        };
    }

    /// <summary>
    /// A character of the portal: its portrait, its level and its copies when the user has it, its shadow otherwise.
    /// </summary>
    private void FillCell(RectTransform cell, CharacterAPI character, GameCharacter owned)
    {
        Color rankColor = CharacterRank.Color(character.rank);
        RectTransform frame = UIBuilder.Child("Frame", cell, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
        UIBuilder.Image(frame, null, new Color(rankColor.r, rankColor.g, rankColor.b, owned != null ? 0.45f : 0.18f), false);

        Image portrait = UIBuilder.Image(UIBuilder.Child("Portrait", frame, new Vector2(0.08f, 0.2f), new Vector2(0.92f, 0.97f)),
            CharacterArt.Portrait(character.num), owned != null ? Color.white : unknownColor, false);
        portrait.preserveAspect = true;
        portrait.enabled = portrait.sprite != null;

        Text rank = UIBuilder.Text(UIBuilder.Child("Rank", frame, new Vector2(0.05f, 0.01f), new Vector2(0.6f, 0.21f)),
            UIBuilder.ModelText, character.rank, 34, TextAnchor.MiddleLeft);
        rank.color = rankColor;
        if (owned == null)
        {
            Text unknown = UIBuilder.Text(UIBuilder.Child("Unknown", frame, new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.85f)),
                UIBuilder.ModelText, "?", 90, TextAnchor.MiddleCenter);
            unknown.color = new Color(1f, 1f, 1f, 0.7f);
        }
        else
        {
            string state = "Nv " + owned.level + (owned.copies > 1 ? "  x" + owned.copies : "");
            Text level = UIBuilder.Text(UIBuilder.Child("Level", frame, new Vector2(0.4f, 0.01f), new Vector2(0.95f, 0.21f)),
                UIBuilder.ModelText, state, 30, TextAnchor.MiddleRight);
            level.color = Color.white;
            if (owned.IsFavorite)
            {
                Image star = UIBuilder.Image(UIBuilder.Child("Favorite", frame, new Vector2(0.72f, 0.78f), new Vector2(0.96f, 0.97f)),
                    CharacterFilterBar.Star, CharacterFilterBar.FavoriteColor, false);
                star.preserveAspect = true;
            }
        }
    }

    private void Draw(int count)
    {
        if (portal == null || count < 1 || CharacterManager.Instance.isBusy || SummonAnimation.IsPlaying) return;
        if (Inventory.Instance.CurrentMoney < portal.cost * count)
        {
            MessagePanel.instance.DisplayMessage("Pas assez d'argent",
                "Il faut " + UIBuilder.Number(portal.cost * count) + " pour cette invocation.");
            return;
        }
        CharacterManager.Instance.Recruit(portal, count, response =>
        {
            if (response == null || response.draws == null || response.draws.Count == 0) return;
            // the server has answered: the animation hints at what was drawn, then shows it
            SummonAnimation.Play(panel.transform, response.draws, () =>
            {
                if (IsOpen()) Render();
                TeamPanel.instance.RenderIfOpen();
            });
        });
    }
}
