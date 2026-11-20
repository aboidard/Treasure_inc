using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The quests: a few objectives drawn by the server each day, and a few longer ones each week. Each quest shows
/// how far it is and what it gives; the chest of a completed quest gives its reward (money and items) when it is
/// clicked. A reward that was not taken is lost when new quests replace the old ones.
/// Under the daily quests, the recap of the daily bonus counts how many of them are completed: when they all are,
/// its chest gives gems.
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
    public static readonly Color chestColor = new Color(1f, 0.72f, 0.1f);
    public static readonly Color chestDetailColor = new Color(0.36f, 0.2f, 0.04f);
    private static readonly Color lockedChestColor = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color lockedDetailColor = new Color(0f, 0f, 0f, 0.3f);

    private static Sprite chestBody;
    private static Sprite chestDetail;
    private static Sprite check;

    private static readonly Color bonusBackColor = new Color(0.42f, 0.2f, 0.02f, 0.55f);
    private static readonly Color bonusTitleColor = new Color(1f, 0.82f, 0.25f);
    private static readonly Color bonusRewardColor = new Color(1f, 0.93f, 0.6f);
    private static readonly Color pipOffColor = new Color(0f, 0f, 0f, 0.55f);
    // how long a gem of the counter takes to settle when it lights up
    private const float popDuration = 0.45f;

    private RectTransform dailyList;
    private RectTransform weeklyList;
    // the recap of the daily bonus, under the daily quests
    private RectTransform bonusRecap;
    private RectTransform bonusReward;
    private RectTransform bonusGem;
    private RectTransform bonusChest;
    private readonly List<RectTransform> pips = new List<RectTransform>();
    private bool bonusWaiting;
    private bool bonusFull;
    // what the counter showed last time, so that the gems lit since then pop; -1 before the first rendering
    private int shownProgress = -1;
    private string shownPeriod;
    private float popStart = -10f;
    private int popFrom;
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
        dailyList = UIBuilder.Child("Daily", root, new Vector2(0.04f, 0.625f), new Vector2(0.96f, 0.865f));
        // right after the daily quests, the recap of the bonus they give together
        bonusRecap = UIBuilder.Child("DailyBonus", root, new Vector2(0.04f, 0.5f), new Vector2(0.96f, 0.618f));

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
        AnimateBonus();
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
        FillBonus(quests != null && quests.daily != null ? quests.daily : null);
        Fill(weeklyList, quests != null ? quests.weekly : null);
        RenderTimers();
    }

    // ---- the daily bonus: a counter of the daily quests completed, and its reward in gems

    /// <summary>A sentence for each step of the counter, from nothing done to the bonus taken.</summary>
    private static string BonusMood(QuestAPI bonus)
    {
        if (bonus.claimed) return "Gemmes empochées. Revenez demain pour remettre ça !";
        if (bonus.completed) return "JACKPOT ! Le coffre à gemmes vous attend !";
        int left = bonus.goal - bonus.progress;
        if (bonus.progress == 0) return "Au boulot, chasseur de trésors !";
        if (left == 1) return "Plus qu'une ! Ça sent la gemme...";
        return "Encore " + left + " quêtes et les gemmes sont à vous !";
    }

    /// <summary>
    /// The recap of the daily bonus: its counter (a gem lights up for each daily quest completed), a sentence that
    /// follows it, and its reward, big, with the chest that gives it once the counter is full.
    /// </summary>
    private void FillBonus(QuestBoardAPI daily)
    {
        for (int i = bonusRecap.childCount - 1; i >= 0; i--)
        {
            Destroy(bonusRecap.GetChild(i).gameObject);
        }
        pips.Clear();
        bonusReward = null;
        bonusGem = null;
        bonusChest = null;
        bonusWaiting = false;
        bonusFull = false;
        Image back = bonusRecap.GetComponent<Image>();
        if (back == null) back = UIBuilder.Image(bonusRecap, null, bonusBackColor, false);
        QuestAPI bonus = daily != null ? daily.bonus : null;
        back.enabled = bonus != null;
        if (bonus == null) return;

        Text model = UIBuilder.ModelText;
        Text title = UIBuilder.Text(UIBuilder.Child("Title", bonusRecap, new Vector2(0.03f, 0.66f), new Vector2(0.62f, 0.97f)),
            model, "Super-bonus du jour", 40, TextAnchor.MiddleLeft);
        title.color = bonusTitleColor;
        AddShadow(title);
        Text mood = UIBuilder.Text(UIBuilder.Child("Mood", bonusRecap, new Vector2(0.03f, 0.42f), new Vector2(0.62f, 0.66f)),
            model, BonusMood(bonus), 28, TextAnchor.MiddleLeft);
        mood.color = Color.white;

        // the counter: one gem for each daily quest to complete, lit when it is
        int goal = Mathf.Max(1, bonus.goal);
        RectTransform counter = UIBuilder.Child("Counter", bonusRecap, new Vector2(0.03f, 0.04f), new Vector2(0.38f, 0.42f));
        for (int i = 0; i < goal; i++)
        {
            RectTransform pip = UIBuilder.Child("Pip" + i, counter, new Vector2(i / (float)goal, 0f), new Vector2((i + 1) / (float)goal, 1f));
            Image gem = UIBuilder.Image(pip, Gems.Icon, i < bonus.progress ? Color.white : pipOffColor, false);
            gem.preserveAspect = true;
            pips.Add(pip);
        }
        Text count = UIBuilder.Text(UIBuilder.Child("Count", bonusRecap, new Vector2(0.39f, 0.02f), new Vector2(0.62f, 0.44f)),
            model, bonus.progress + " / " + goal, 52, TextAnchor.MiddleCenter);
        count.color = bonus.completed ? doneColor : Color.white;
        AddShadow(count);

        // the reward, in the spotlight
        bonusReward = UIBuilder.Child("Reward", bonusRecap, new Vector2(0.63f, 0.5f), new Vector2(0.85f, 0.97f));
        int gems = bonus.reward != null ? bonus.reward.gems : 0;
        Text reward = UIBuilder.Text(bonusReward, model, "+" + UIBuilder.Number(gems), 60, TextAnchor.MiddleRight);
        reward.color = bonus.claimed ? new Color(1f, 1f, 1f, 0.5f) : bonusRewardColor;
        AddShadow(reward);
        Image icon = Gems.AddIcon(bonusRecap, new Vector2(0.86f, 0.52f), new Vector2(0.985f, 0.95f));
        bonusGem = icon.rectTransform;
        if (bonus.claimed) icon.color = new Color(1f, 1f, 1f, 0.5f);

        RectTransform claim = UIBuilder.Child("Claim", bonusRecap, new Vector2(0.63f, 0.03f), new Vector2(0.985f, 0.5f));
        if (bonus.claimed)
        {
            Image done = UIBuilder.Image(UIBuilder.Child("Check", claim, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(88f, 56f)),
                Check, doneColor, false);
            done.preserveAspect = true;
        }
        else
        {
            bool claimable = bonus.IsClaimable;
            Image hit = UIBuilder.Image(claim, null, new Color(0f, 0f, 0f, 0f), claimable);
            bonusChest = UIBuilder.Child("Chest", claim, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(112f, 84f));
            UIBuilder.Image(bonusChest, ChestBody, claimable ? chestColor : lockedChestColor, false);
            UIBuilder.Image(UIBuilder.Child("Detail", bonusChest, Vector2.zero, Vector2.one),
                ChestDetail, claimable ? chestDetailColor : lockedDetailColor, false);
            if (claimable)
            {
                Button button = claim.gameObject.AddComponent<Button>();
                button.targetGraphic = hit;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => Claim(bonus));
                bonusWaiting = true;
            }
        }
        bonusFull = bonus.completed && !bonus.claimed;

        // the gems lit since the last time pop; a new day starts the counter again without a pop
        if (shownPeriod == daily.period && shownProgress >= 0 && bonus.progress > shownProgress)
        {
            popFrom = shownProgress;
            popStart = Time.unscaledTime;
        }
        shownPeriod = daily.period;
        shownProgress = bonus.progress;
    }

    private void AnimateBonus()
    {
        float time = Time.unscaledTime;
        if (bonusReward != null)
        {
            // the reward always breathes; much more when it waits to be taken
            float amplitude = bonusWaiting ? 0.1f : 0.035f;
            bonusReward.localScale = Vector3.one * (1f + amplitude * Mathf.Sin(time * (bonusWaiting ? 6f : 2.5f)));
        }
        if (bonusGem != null)
        {
            bonusGem.localRotation = Quaternion.Euler(0f, 0f, 12f * Mathf.Sin(time * 3f));
        }
        if (bonusChest != null && bonusWaiting)
        {
            // the chest jumps up and down, and shakes a little
            bonusChest.localScale = Vector3.one * (1f + 0.08f * Mathf.Abs(Mathf.Sin(time * 5f)));
            bonusChest.localRotation = Quaternion.Euler(0f, 0f, 6f * Mathf.Sin(time * 14f));
        }
        for (int i = 0; i < pips.Count; i++)
        {
            if (pips[i] == null) continue;
            float scale = 1f;
            float since = time - popStart;
            if (i >= popFrom && i < shownProgress && since < popDuration)
            {
                // a gem that just lit up: big, then it settles with a bounce
                float t = since / popDuration;
                scale = 1f + 0.8f * (1f - t) * Mathf.Abs(Mathf.Cos(t * Mathf.PI * 2f));
            }
            else if (bonusFull)
            {
                // the counter is full: a wave runs through its gems
                scale = 1f + 0.12f * Mathf.Max(0f, Mathf.Sin(time * 4f - i * 0.9f));
            }
            pips[i].localScale = Vector3.one * scale;
        }
    }

    private static void AddShadow(Text text)
    {
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);
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
            int gems = claimed.reward != null ? claimed.reward.gems : 0;
            string gains = (money > 0 ? "  +" + UIBuilder.Number(money) + " pièces" : "")
                + (gems > 0 ? "  +" + Gems.Amount(gems) : "");
            string title = quest.id == Quest.BonusId ? "Super-bonus du jour !" : "Quête accomplie !";
            LootPanel.instance.ShowItems(title + gains, items);
        });
    }

    // ---- the icons, drawn in two layers so that the chest and its fittings have their own color

    /// <summary>The chest of a reward to take (the quests, the gift box): its body...</summary>
    public static Sprite ChestBody
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

    /// <summary>...and its fittings, drawn over it in another color.</summary>
    public static Sprite ChestDetail
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
