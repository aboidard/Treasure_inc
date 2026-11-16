using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The quests: a few objectives drawn by the server each day, and a few longer ones each week. Each quest shows
/// how far it is and what it gives; the chest of a completed quest gives its reward (money and items) when it is
/// clicked. A reward that was not taken is lost when new quests replace the old ones.
/// The scene has no such panel: it is built by code with the frame of the crew panel (see Create), and opened by
/// the QuestButton at the top left of the screen.
/// </summary>
public class QuestPanel : Panel
{
    public static QuestPanel instance;

    // a board holds this many quests, or more: fewer of them keep the size of their row
    private const int minRows = 3;
    private const float timerPeriod = 0.5f;
    private static readonly Color rowColor = new Color(0f, 0f, 0f, 0.3f);
    private static readonly Color barBackColor = new Color(0f, 0f, 0f, 0.45f);
    private static readonly Color barColor = new Color(1f, 0.75f, 0f);
    private static readonly Color doneColor = new Color(0.4f, 0.9f, 0.4f);
    private static readonly Color rewardColor = new Color(1f, 0.88f, 0.45f);
    private static readonly Color chestColor = new Color(1f, 0.72f, 0.1f);
    private static readonly Color chestDetailColor = new Color(0.36f, 0.2f, 0.04f);
    private static readonly Color lockedChestColor = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color lockedDetailColor = new Color(0f, 0f, 0f, 0.3f);

    private static Sprite chestBody;
    private static Sprite chestDetail;
    private static Sprite check;

    private RectTransform dailyList;
    private RectTransform weeklyList;
    private Text dailyTimer;
    private Text weeklyTimer;
    // the chests that can be opened: they beat
    private readonly List<RectTransform> waiting = new List<RectTransform>();
    private float nextTimerUpdate;
    private QuestManager manager;

    /// <summary>Creates the panel next to the crew panel, with its place, its size and its frame, and its button.</summary>
    public static QuestPanel Create(GameObject crewPanel)
    {
        RectTransform source = crewPanel.GetComponent<RectTransform>();
        GameObject created = new GameObject("QuestPanel", typeof(RectTransform));
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

        QuestPanel quests = created.AddComponent<QuestPanel>();
        quests.panel = created;
        // the panels close each other through this list
        List<Panel> panels = UIManager.instance.panels.ToList();
        panels.Add(quests);
        UIManager.instance.panels = panels.ToArray();

        QuestButton.Create(crewPanel.GetComponentInParent<Canvas>().rootCanvas);
        return quests;
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
            model, "Quêtes", 73, TextAnchor.MiddleCenter);

        UIBuilder.Text(UIBuilder.Child("DailyTitle", root, new Vector2(0.05f, 0.87f), new Vector2(0.5f, 0.915f)),
            model, "Quotidiennes", 46, TextAnchor.MiddleLeft);
        dailyTimer = UIBuilder.Text(UIBuilder.Child("DailyTimer", root, new Vector2(0.5f, 0.87f), new Vector2(0.95f, 0.915f)),
            model, "", 32, TextAnchor.MiddleRight);
        dailyList = UIBuilder.Child("Daily", root, new Vector2(0.04f, 0.505f), new Vector2(0.96f, 0.865f));

        UIBuilder.Text(UIBuilder.Child("WeeklyTitle", root, new Vector2(0.05f, 0.445f), new Vector2(0.5f, 0.49f)),
            model, "Hebdomadaires", 46, TextAnchor.MiddleLeft);
        weeklyTimer = UIBuilder.Text(UIBuilder.Child("WeeklyTimer", root, new Vector2(0.5f, 0.445f), new Vector2(0.95f, 0.49f)),
            model, "", 32, TextAnchor.MiddleRight);
        weeklyList = UIBuilder.Child("Weekly", root, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.44f));

        UIBuilder.Text(UIBuilder.Child("Hint", root, new Vector2(0.05f, 0.025f), new Vector2(0.95f, 0.07f)),
            model, "Ouvrez le coffre d'une quête terminée avant l'arrivée des nouvelles quêtes !", 30, TextAnchor.MiddleCenter);

        manager = QuestManager.Instance;
        manager.Changed += OnQuestsChanged;
        QuestManager.Instance.Refresh();
    }

    private void OnDestroy()
    {
        // not through Instance, which would create a manager while the game is closing
        if (manager != null) manager.Changed -= OnQuestsChanged;
    }

    private void OnQuestsChanged()
    {
        if (IsOpen()) Render();
    }

    protected override void WillShow()
    {
        Render();
        QuestManager.Instance.Refresh();
    }

    void Update()
    {
        float beat = 1f + 0.07f * Mathf.Sin(Time.unscaledTime * 5f);
        foreach (RectTransform chest in waiting)
        {
            if (chest != null) chest.localScale = Vector3.one * beat;
        }
        if (Time.unscaledTime >= nextTimerUpdate)
        {
            nextTimerUpdate = Time.unscaledTime + timerPeriod;
            RenderTimers();
        }
    }

    private void Render()
    {
        waiting.Clear();
        QuestsAPI quests = QuestManager.Instance.quests;
        Fill(dailyList, quests != null ? quests.daily : null);
        Fill(weeklyList, quests != null ? quests.weekly : null);
        RenderTimers();
    }

    private void RenderTimers()
    {
        QuestsAPI quests = QuestManager.Instance.quests;
        dailyTimer.text = quests != null ? Timer(quests.daily) : "";
        weeklyTimer.text = quests != null ? Timer(quests.weekly) : "";
    }

    private static string Timer(QuestBoardAPI board)
    {
        float left = QuestManager.Instance.SecondsLeft(board);
        return left > 0f ? "Nouvelles quêtes dans " + Quest.Duration(left) : "Nouvelles quêtes...";
    }

    private void Fill(RectTransform list, QuestBoardAPI board)
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }
        if (board == null || board.quests.Count == 0)
        {
            UIBuilder.Text(UIBuilder.Child("Empty", list, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.65f)),
                UIBuilder.ModelText, board == null ? "Chargement..." : "Aucune quête pour le moment.", 40, TextAnchor.MiddleCenter);
            return;
        }
        int rows = Mathf.Max(minRows, board.quests.Count);
        for (int i = 0; i < board.quests.Count; i++)
        {
            RectTransform row = UIBuilder.Child("Quest" + i, list,
                new Vector2(0f, 1f - (i + 1) / (float)rows), new Vector2(1f, 1f - i / (float)rows));
            // a gap between two rows
            row.offsetMin = new Vector2(0f, 6f);
            row.offsetMax = new Vector2(0f, -6f);
            FillRow(row, board.quests[i]);
        }
    }

    /// <summary>A quest: its sentence, its progress on a bar, its reward and its chest.</summary>
    private void FillRow(RectTransform row, QuestAPI quest)
    {
        UIBuilder.Image(row, null, rowColor, false);
        Text model = UIBuilder.ModelText;
        float alpha = quest.claimed ? 0.55f : 1f;

        Text label = UIBuilder.Text(UIBuilder.Child("Label", row, new Vector2(0.03f, 0.54f), new Vector2(0.75f, 0.95f)),
            model, Quest.Describe(quest), 40, TextAnchor.MiddleLeft);
        label.color = new Color(1f, 1f, 1f, alpha);

        RectTransform bar = UIBuilder.Child("Bar", row, new Vector2(0.03f, 0.3f), new Vector2(0.75f, 0.5f));
        UIBuilder.Image(bar, null, barBackColor, false);
        float ratio = quest.goal > 0 ? Mathf.Clamp01(quest.progress / (float)quest.goal) : 1f;
        Color fillColor = quest.completed ? doneColor : barColor;
        UIBuilder.Image(UIBuilder.Child("Fill", bar, Vector2.zero, new Vector2(ratio, 1f)),
            null, new Color(fillColor.r, fillColor.g, fillColor.b, alpha), false);
        Text progress = UIBuilder.Text(UIBuilder.Child("Progress", bar, Vector2.zero, Vector2.one), model,
            UIBuilder.Number(quest.progress) + " / " + UIBuilder.Number(quest.goal), 30, TextAnchor.MiddleCenter);
        progress.color = Color.white;
        Shadow shadow = progress.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);

        string state = quest.claimed ? "Récompense récupérée" : "Récompense : " + Quest.Reward(quest.reward);
        Text reward = UIBuilder.Text(UIBuilder.Child("Reward", row, new Vector2(0.03f, 0.05f), new Vector2(0.75f, 0.27f)),
            model, state, 30, TextAnchor.MiddleLeft);
        reward.color = new Color(rewardColor.r, rewardColor.g, rewardColor.b, alpha);

        RectTransform claim = UIBuilder.Child("Claim", row, new Vector2(0.77f, 0.06f), new Vector2(0.985f, 0.94f));
        if (quest.claimed)
        {
            Image done = UIBuilder.Image(UIBuilder.Child("Check", claim, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(110f, 70f)),
                Check, doneColor, false);
            done.preserveAspect = true;
            return;
        }

        bool claimable = quest.IsClaimable;
        // invisible, but the whole corner of the row takes the click
        Image hit = UIBuilder.Image(claim, null, new Color(0f, 0f, 0f, 0f), claimable);
        RectTransform chest = UIBuilder.Child("Chest", claim, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(144f, 108f));
        UIBuilder.Image(chest, ChestBody, claimable ? chestColor : lockedChestColor, false);
        UIBuilder.Image(UIBuilder.Child("Detail", chest, Vector2.zero, Vector2.one),
            ChestDetail, claimable ? chestDetailColor : lockedDetailColor, false);
        if (!claimable) return;

        Button button = claim.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => Claim(quest));
        waiting.Add(chest);
    }

    private void Claim(QuestAPI quest)
    {
        if (QuestManager.Instance.isBusy) return;
        SoundManager.Play(Sfx.ChestOpen);
        QuestManager.Instance.Claim(quest, (claimed, items) =>
        {
            if (claimed == null) return;
            SoundManager.Play(Sfx.Coins);
            int money = claimed.reward != null ? claimed.reward.money : 0;
            LootPanel.instance.ShowItems("Quête accomplie !  +" + UIBuilder.Number(money) + " pièces", items);
        });
    }

    // ---- the icons, drawn in two layers so that the chest and its fittings have their own color

    private static Sprite ChestBody
    {
        get
        {
            if (chestBody == null)
            {
                chestBody = UIBuilder.PixelSprite(
                    "..############..",
                    ".##############.",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################",
                    "################");
            }
            return chestBody;
        }
    }

    private static Sprite ChestDetail
    {
        get
        {
            if (chestDetail == null)
            {
                chestDetail = UIBuilder.PixelSprite(
                    "................",
                    "................",
                    "..#..........#..",
                    "..#..........#..",
                    "..#...####...#..",
                    "#######..#######",
                    "..#...#..#...#..",
                    "..#...####...#..",
                    "..#..........#..",
                    "..#..........#..",
                    "..#..........#..",
                    "................");
            }
            return chestDetail;
        }
    }

    private static Sprite Check
    {
        get
        {
            if (check == null)
            {
                check = UIBuilder.PixelSprite(
                    ".........##",
                    "........##.",
                    ".......##..",
                    "##....##...",
                    ".##..##....",
                    "..####.....",
                    "...##......");
            }
            return check;
        }
    }
}
