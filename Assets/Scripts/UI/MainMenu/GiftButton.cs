using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The small button of the gift box, at the top of the screen right next to the button of the quests: it opens the
/// gift box (GiftPanel), and a badge counts the gifts waiting in it. Built by code in the HUD of the scene.
/// </summary>
public class GiftButton : MonoBehaviour
{
    private const float size = 128f;
    // the button of the quests is at the left of the HUD; this one follows it
    private const float gap = 16f;
    private static readonly Color badgeColor = new Color(0.88f, 0.14f, 0.14f);

    private static Sprite present;
    private static Sprite disc;

    private RectTransform icon;
    private RectTransform badge;
    private Text count;
    private GiftManager manager;

    /// <summary>A present: a box with a ribbon and a bow.</summary>
    private static Sprite Present
    {
        get
        {
            if (present == null)
            {
                present = UIBuilder.PixelSprite(
                    "..##....##..",
                    ".#..#..#..#.",
                    ".#...##...#.",
                    "..#..##..#..",
                    "############",
                    "#....##....#",
                    "############",
                    ".#...##...#.",
                    ".#...##...#.",
                    ".#...##...#.",
                    ".#...##...#.",
                    ".##########.");
            }
            return present;
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

    public static GiftButton Create(Canvas canvas)
    {
        Transform hud = canvas.transform.Find("HUDPanel");
        float x = size / 2f + size + gap;
        RectTransform rect = hud != null
            ? UIBuilder.Child("GiftButton", hud, new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(size, size))
            // without the HUD of the scene: the top left corner of the screen, next to the quests
            : UIBuilder.Child("GiftButton", canvas.transform, new Vector2(0f, 1f), new Vector2(110f + size + gap, -113f), new Vector2(size, size));
        GiftButton button = rect.gameObject.AddComponent<GiftButton>();
        button.Build(rect);
        return button;
    }

    private void Build(RectTransform rect)
    {
        UIBuilder.Button(rect, "", 10, () => GiftPanel.instance.Show());

        icon = UIBuilder.Child("Icon", rect, new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.76f));
        Image image = UIBuilder.Image(icon, Present, UIBuilder.ButtonTextColor, false);
        image.preserveAspect = true;

        badge = UIBuilder.Child("Badge", rect, new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(62f, 62f));
        UIBuilder.Image(badge, Disc, badgeColor, false);
        count = UIBuilder.Text(UIBuilder.Child("Count", badge, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.9f)),
            UIBuilder.ModelText, "", 40, TextAnchor.MiddleCenter);
        count.color = Color.white;

        manager = GiftManager.Instance;
        manager.Changed += Render;
        Render();
        // the gifts sent while the game was closed
        manager.Refresh();
    }

    private void OnDestroy()
    {
        // not through Instance, which would create a manager while the game is closing
        if (manager != null) manager.Changed -= Render;
    }

    private void Render()
    {
        int waiting = GiftManager.Instance.Count;
        badge.gameObject.SetActive(waiting > 0);
        count.text = waiting > 99 ? "99+" : waiting.ToString();
    }

    private void Update()
    {
        if (!badge.gameObject.activeSelf) return;
        // the badge beats and the present wiggles while a gift waits
        float time = Time.unscaledTime;
        badge.localScale = Vector3.one * (1f + 0.1f * Mathf.Abs(Mathf.Sin(time * 3f)));
        // a short shake every two seconds or so
        float shake = Mathf.Repeat(time, 2.2f) < 0.4f ? Mathf.Sin(time * 40f) * 8f : 0f;
        icon.localRotation = Quaternion.Euler(0f, 0f, shake);
    }
}
