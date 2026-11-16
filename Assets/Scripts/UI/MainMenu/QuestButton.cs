using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The small button of the quests, at the top left of the screen: it opens the quests (QuestPanel), and a badge
/// counts the completed quests whose reward waits. Built by code at the left of the HUD of the scene, which holds
/// the money at its right.
/// </summary>
public class QuestButton : MonoBehaviour
{
    private const float size = 128f;
    private static readonly Color badgeColor = new Color(0.88f, 0.14f, 0.14f);

    private static Sprite todoList;
    private static Sprite disc;

    private RectTransform icon;
    private RectTransform badge;
    private Text count;
    private QuestManager manager;

    /// <summary>A sheet with three lines to tick.</summary>
    private static Sprite TodoList
    {
        get
        {
            if (todoList == null)
            {
                todoList = UIBuilder.PixelSprite(
                    "############",
                    "#..........#",
                    "#.##.#####.#",
                    "#.##.#####.#",
                    "#..........#",
                    "#.##.#####.#",
                    "#.##.#####.#",
                    "#..........#",
                    "#.##.###...#",
                    "#.##.###...#",
                    "#..........#",
                    "############");
            }
            return todoList;
        }
    }

    private static Sprite Disc
    {
        get
        {
            if (disc == null)
            {
                disc = UIBuilder.PixelSprite(
                    "..####..",
                    ".######.",
                    "########",
                    "########",
                    "########",
                    "########",
                    ".######.",
                    "..####..");
            }
            return disc;
        }
    }

    public static QuestButton Create(Canvas canvas)
    {
        Transform hud = canvas.transform.Find("HUDPanel");
        RectTransform rect = hud != null
            ? UIBuilder.Child("QuestButton", hud, new Vector2(0f, 0.5f), new Vector2(size / 2f, 0f), new Vector2(size, size))
            // without the HUD of the scene: the top left corner of the screen
            : UIBuilder.Child("QuestButton", canvas.transform, new Vector2(0f, 1f), new Vector2(110f, -113f), new Vector2(size, size));
        QuestButton button = rect.gameObject.AddComponent<QuestButton>();
        button.Build(rect);
        return button;
    }

    private void Build(RectTransform rect)
    {
        UIBuilder.Button(rect, "", 10, () => QuestPanel.instance.Show());

        icon = UIBuilder.Child("Icon", rect, new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.76f));
        Image image = UIBuilder.Image(icon, TodoList, UIBuilder.ButtonTextColor, false);
        image.preserveAspect = true;

        badge = UIBuilder.Child("Badge", rect, new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(62f, 62f));
        UIBuilder.Image(badge, Disc, badgeColor, false);
        count = UIBuilder.Text(UIBuilder.Child("Count", badge, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f)),
            UIBuilder.ModelText, "", 40, TextAnchor.MiddleCenter);
        count.color = Color.white;

        manager = QuestManager.Instance;
        manager.Changed += Render;
        Render();
    }

    private void OnDestroy()
    {
        // not through Instance, which would create a manager while the game is closing
        if (manager != null) manager.Changed -= Render;
    }

    private void Render()
    {
        int claimable = QuestManager.Instance.Claimable;
        badge.gameObject.SetActive(claimable > 0);
        count.text = claimable.ToString();
    }

    private void Update()
    {
        // the badge beats while a reward waits
        if (badge.gameObject.activeSelf)
        {
            badge.localScale = Vector3.one * (1f + 0.1f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f)));
        }
    }
}
