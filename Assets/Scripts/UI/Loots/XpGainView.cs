using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// What an expedition brought to its character, shown below the items of its chest (see LootPanel): its portrait,
/// its level, the experience gained on a bar that fills, and the stats of the levels gained. Built by code.
/// </summary>
public class XpGainView : MonoBehaviour
{
    public const float Height = 300f;

    private const float fillTime = 0.7f;
    private const float fullLevelTime = 0.25f;
    // a character gaining many levels at once does not make the user wait for each of them
    private const int maxLevelsShown = 4;
    private static readonly Color barBackColor = new Color(0f, 0f, 0f, 0.35f);
    private static readonly Color barColor = new Color(1f, 0.75f, 0f);
    private const string goodColor = "#1E7B1E";

    private RectTransform fill;
    private float startRatio;
    private float endRatio;
    private int levelsGained;

    /// <param name="before">the character as it was before the gain; null when it is not known</param>
    /// <param name="model">a text whose font and color are used</param>
    public static XpGainView Create(Transform parent, Text model, XpGainAPI gain, GameCharacter before)
    {
        RectTransform rect = UIBuilder.Child("XpGain", parent, new Vector2(0f, 1f), new Vector2(1f, 1f));
        LayoutElement size = rect.gameObject.AddComponent<LayoutElement>();
        size.minHeight = Height;
        size.preferredHeight = Height;
        XpGainView view = rect.gameObject.AddComponent<XpGainView>();
        view.Build(rect, model, gain, before);
        return view;
    }

    private void Build(RectTransform rect, Text model, XpGainAPI gain, GameCharacter before)
    {
        bool levelUp = gain.level > gain.levelBefore;
        bool max = gain.xpNext <= 0;
        levelsGained = Mathf.Max(0, gain.level - gain.levelBefore);
        endRatio = max ? 1f : Mathf.Clamp01(gain.xp / (float)gain.xpNext);
        if (before != null && before.level == gain.levelBefore)
        {
            startRatio = before.xpNext <= 0 ? 1f : Mathf.Clamp01(before.xp / (float)before.xpNext);
        }
        else
        {
            startRatio = levelUp || max ? 0f : Mathf.Clamp01((gain.xp - gain.gained) / (float)gain.xpNext);
        }

        // a line between the items and the experience
        UIBuilder.Image(UIBuilder.Child("Line", rect, new Vector2(0.02f, 0.97f), new Vector2(0.98f, 0.98f)),
            null, new Color(model.color.r, model.color.g, model.color.b, 0.4f), false);

        Sprite graphics = before != null ? before.graphics : CharacterArt.Portrait(gain.character);
        Image portrait = UIBuilder.Image(UIBuilder.Child("Portrait", rect, new Vector2(0.02f, 0.06f), new Vector2(0.26f, 0.92f)),
            graphics, Color.white, false);
        portrait.preserveAspect = true;
        portrait.enabled = graphics != null;

        string name = before != null ? before.name : "Personnage " + gain.character;
        string level = levelUp
            ? "Niveau " + gain.levelBefore + " > <color=" + goodColor + ">" + gain.level + " !</color>"
            : "Niveau " + gain.level;
        UIBuilder.Text(UIBuilder.Child("Name", rect, new Vector2(0.3f, 0.6f), new Vector2(0.98f, 0.93f)),
            model, name + "\n" + level, 46, TextAnchor.MiddleLeft);

        RectTransform bar = UIBuilder.Child("Bar", rect, new Vector2(0.3f, 0.44f), new Vector2(0.98f, 0.56f));
        UIBuilder.Image(bar, null, barBackColor, false);
        fill = UIBuilder.Child("Fill", bar, Vector2.zero, new Vector2(startRatio, 1f));
        UIBuilder.Image(fill, null, barColor, false);

        string points = max ? "Niveau maximum" : UIBuilder.Number(gain.xp) + " / " + UIBuilder.Number(gain.xpNext) + " XP";
        UIBuilder.Text(UIBuilder.Child("Gained", rect, new Vector2(0.3f, 0.29f), new Vector2(0.6f, 0.42f)),
            model, "+" + UIBuilder.Number(gain.gained) + " XP", 38, TextAnchor.MiddleLeft);
        UIBuilder.Text(UIBuilder.Child("Points", rect, new Vector2(0.6f, 0.29f), new Vector2(0.98f, 0.42f)),
            model, points, 34, TextAnchor.MiddleRight);

        List<string> stats = levelUp
            ? Equipment.Compare(gain.statsBefore, gain.stats).Select(change => change.text).ToList()
            : new List<string>();
        if (stats.Count > 0)
        {
            UIBuilder.Text(UIBuilder.Child("Stats", rect, new Vector2(0.3f, 0.05f), new Vector2(0.98f, 0.27f)),
                model, "<color=" + goodColor + ">" + string.Join("   ", stats) + "</color>", 36, TextAnchor.MiddleLeft);
        }

        // OnEnable came before the bar existed
        if (isActiveAndEnabled) StartCoroutine(Animate());
    }

    private void OnEnable()
    {
        // shown again after its panel was closed: the bar fills again
        if (fill != null) StartCoroutine(Animate());
    }

    /// <summary>The bar fills up to the experience gained; it starts again from empty at each level gained.</summary>
    private IEnumerator Animate()
    {
        SetFill(startRatio);
        yield return new WaitForSecondsRealtime(0.4f);

        float from = startRatio;
        for (int i = 0; i < Mathf.Min(levelsGained, maxLevelsShown); i++)
        {
            yield return FillTo(from, 1f, i == 0 ? fillTime : fullLevelTime);
            SoundManager.Play(Sfx.RankStamp, 1f + 0.1f * i);
            from = 0f;
        }
        yield return FillTo(from, endRatio, fillTime);
    }

    private IEnumerator FillTo(float from, float to, float duration)
    {
        for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
        {
            SetFill(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, time / duration)));
            yield return null;
        }
        SetFill(to);
    }

    private void SetFill(float ratio)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(ratio), 1f);
    }
}
