using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A barricade or a monster standing in the way of an expedition (see ExpeditionRunPanel): its shape, its name
/// and its life on a bar. Built by code, like the texts that fly over a fight.
/// </summary>
public class EncounterView : MonoBehaviour
{
    private static readonly Color barBackColor = new Color(0f, 0f, 0f, 0.6f);
    private static readonly Color barColor = new Color(0.85f, 0.2f, 0.2f);
    private static readonly Color barricadeBarColor = new Color(0.9f, 0.7f, 0.3f);

    private RectTransform rect;
    private RectTransform shape;
    private RectTransform fill;
    private CanvasGroup group;
    private int maxHp;

    /// <param name="position">where its center stands in its parent, from the center of the parent</param>
    /// <param name="height">the height of its shape; a guardian is given more than the others</param>
    /// <param name="model">a text whose font is used</param>
    public static EncounterView Create(Transform parent, Vector2 position, float height, EncounterAPI encounter, Text model)
    {
        RectTransform rect = UIBuilder.Child("Encounter", parent, new Vector2(0.5f, 0.5f), position, new Vector2(height, height));
        EncounterView view = rect.gameObject.AddComponent<EncounterView>();
        view.Build(rect, encounter, model);
        return view;
    }

    private void Build(RectTransform rect, EncounterAPI encounter, Text model)
    {
        this.rect = rect;
        group = rect.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;
        maxHp = Mathf.Max(1, encounter.hp);

        string kind = encounter.IsBarricade || encounter.monster == null ? MonsterArt.BARRICADE : encounter.monster.kind;
        shape = UIBuilder.Child("Shape", rect, Vector2.zero, Vector2.one);
        Image image = UIBuilder.Image(shape, MonsterArt.Sprite(kind), MonsterArt.Color(kind), false);
        image.preserveAspect = true;
        // the monsters look at the character, who comes from the left: nothing to flip, the shapes are symmetric

        RectTransform bar = UIBuilder.Child("Bar", rect, new Vector2(0.05f, 1.04f), new Vector2(0.95f, 1.14f));
        UIBuilder.Image(bar, null, barBackColor, false);
        fill = UIBuilder.Child("Fill", bar, Vector2.zero, Vector2.one);
        UIBuilder.Image(fill, null, encounter.IsBarricade ? barricadeBarColor : barColor, false);

        string name = encounter.IsBarricade || encounter.monster == null ? "Barricade" : encounter.monster.name;
        Text label = UIBuilder.Text(UIBuilder.Child("Name", rect, new Vector2(-0.6f, 1.16f), new Vector2(1.6f, 1.5f)),
            model, name, 22, TextAnchor.LowerCenter);
        label.color = Color.white;
        Outline outline = label.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
    }

    /// <summary>The life left, out of the life the encounter started with.</summary>
    public void SetHp(float hp)
    {
        if (fill != null) fill.anchorMax = new Vector2(Mathf.Clamp01(hp / maxHp), 1f);
    }

    /// <summary>It was hit: its shape shakes for a moment.</summary>
    public void Shake(float strength = 8f)
    {
        if (isActiveAndEnabled) StartCoroutine(Punch(shape, new Vector2(strength, 0f), 0.15f));
    }

    /// <summary>It attacks: its shape jumps at the character, on its left.</summary>
    public void Lunge()
    {
        if (isActiveAndEnabled) StartCoroutine(Punch(shape, new Vector2(-rect.sizeDelta.x * 0.25f, 0f), 0.2f));
    }

    /// <summary>It is destroyed or dead: it fades away.</summary>
    public IEnumerator Vanish(float duration = 0.35f)
    {
        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            float life = time / duration;
            group.alpha = 1f - life;
            rect.localScale = Vector3.one * (1f + 0.3f * life);
            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>Moves a transform, which rests at the center of its anchors, away by an offset and back.</summary>
    public static IEnumerator Punch(RectTransform target, Vector2 offset, float duration)
    {
        if (target == null) yield break;
        Vector2 origin = Vector2.zero;
        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            if (target == null) yield break;
            target.anchoredPosition = origin + offset * Mathf.Sin(time / duration * Mathf.PI);
            yield return null;
        }
        if (target != null) target.anchoredPosition = origin;
    }

    /// <summary>
    /// A text that flies up and fades away, e.g. the damage of a hit or the name of a skill.
    /// </summary>
    /// <param name="position">where it starts in its parent, from the center of the parent</param>
    public static void FloatText(Transform parent, Vector2 position, Text model, string content, Color color, int size)
    {
        RectTransform rect = UIBuilder.Child("FloatingText", parent, new Vector2(0.5f, 0.5f), position, new Vector2(420f, size * 1.5f));
        Text text = UIBuilder.Text(rect, model, content, size, TextAnchor.MiddleCenter);
        text.resizeTextForBestFit = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = color;
        Outline outline = rect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        rect.gameObject.AddComponent<FloatingText>();
    }
}
