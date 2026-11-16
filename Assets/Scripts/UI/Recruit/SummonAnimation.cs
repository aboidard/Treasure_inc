using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The summoning shown once the server has answered a recruitment, over the whole screen:
///   1. the portal charges. It starts with the color of the lowest rank and climbs the ranks one by one up to the
///      best rank of the draws: the longer and the wilder it gets, the better the best character drawn;
///   2. each character comes out, with an entrance as impressive as its rank;
///   3. after several draws, they are all shown together.
/// A touch goes on to the next step, and "Passer" skips to the end. Everything is built and animated by code.
/// </summary>
public class SummonAnimation : MonoBehaviour
{
    private static SummonAnimation instance;

    private const int maxRays = 24;
    private const int maxParticles = 160;
    private static readonly Vector2 center = new Vector2(0f, 60f);
    private static readonly Vector2 portraitSize = new Vector2(432f, 576f);
    private static readonly Color newColor = new Color(1f, 0.92f, 0.3f);
    private static readonly Color duplicateColor = new Color(0.8f, 0.8f, 0.85f);

    private class Particle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 position;
        public Vector2 velocity;
        public float drag;
        public float age;
        public float duration;
        public float size;
        public bool alive;
    }

    private RectTransform stage;
    private RectTransform portal;
    private RectTransform rays;
    private Image glow, core, ringA, ringB, flash;
    private readonly List<Image> rayImages = new List<Image>();
    private readonly List<Particle> particles = new List<Particle>();
    private RectTransform particleLayer;
    private Image portrait;
    private Text rankText, nameText, statusText, hintText, counterText;
    private GameObject skipButton;
    private RectTransform summary;

    // what Update animates; the coroutines only set these values
    private int colorTier;
    private float clock;
    private float spin;
    private float portalScale = 1f;
    private float glowAlpha;
    private float coreAlpha;
    private float rayAlpha;
    private float flashAlpha;
    private float shake;
    private float shakeFloor;
    private bool tapped;
    private bool skip;

    public static bool IsPlaying => instance != null && instance.gameObject.activeSelf;

    /// <summary>Shows the draws in their order, then calls onDone.</summary>
    /// <param name="owner">an element of the canvas the animation is shown on</param>
    public static void Play(Transform owner, List<DrawAPI> draws, Action onDone)
    {
        if (instance == null)
        {
            Canvas canvas = owner.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("SummonAnimation", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<SummonAnimation>();
            instance.Build();
        }
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        instance.StartCoroutine(instance.Run(draws, onDone));
    }

    // ---- construction

    private void Build()
    {
        Text model = UIBuilder.ModelText;
        Vector2 middle = new Vector2(0.5f, 0.5f);

        // the background takes every touch: nothing behind can be clicked meanwhile
        Image background = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0.02f, 0.02f, 0.06f, 0.93f), true);
        Button touch = gameObject.AddComponent<Button>();
        touch.targetGraphic = background;
        touch.transition = Selectable.Transition.None;
        touch.onClick.AddListener(() => tapped = true);

        stage = UIBuilder.Child("Stage", transform, middle, Vector2.zero, Vector2.zero);
        portal = UIBuilder.Child("Portal", stage, middle, center, Vector2.zero);
        rays = UIBuilder.Child("Rays", portal, middle, Vector2.zero, Vector2.zero);
        for (int i = 0; i < maxRays; i++)
        {
            RectTransform ray = UIBuilder.Child("Ray", rays, middle, Vector2.zero, new Vector2(150f, 1500f));
            ray.pivot = new Vector2(0.5f, 0f);
            rayImages.Add(UIBuilder.Image(ray, FxSprites.Ray, Color.white, false));
        }
        glow = UIBuilder.Image(UIBuilder.Child("Glow", portal, middle, Vector2.zero, new Vector2(1000f, 1000f)), FxSprites.Soft, Color.white, false);
        ringA = UIBuilder.Image(UIBuilder.Child("RingA", portal, middle, Vector2.zero, new Vector2(780f, 780f)), FxSprites.RingA, Color.white, false);
        ringB = UIBuilder.Image(UIBuilder.Child("RingB", portal, middle, Vector2.zero, new Vector2(580f, 580f)), FxSprites.RingB, Color.white, false);
        core = UIBuilder.Image(UIBuilder.Child("Core", portal, middle, Vector2.zero, new Vector2(340f, 340f)), FxSprites.Soft, Color.white, false);

        particleLayer = UIBuilder.Child("Particles", stage, middle, center, Vector2.zero);

        portrait = UIBuilder.Image(UIBuilder.Child("Portrait", stage, middle, center, portraitSize), null, Color.white, false);
        portrait.preserveAspect = true;
        rankText = UIBuilder.Text(UIBuilder.Child("Rank", stage, middle, center + new Vector2(0f, 400f), new Vector2(700f, 190f)),
            model, "", 170, TextAnchor.MiddleCenter);
        Outline rankOutline = rankText.gameObject.AddComponent<Outline>();
        rankOutline.effectColor = new Color(0f, 0f, 0f, 0.8f);
        rankOutline.effectDistance = new Vector2(4f, -4f);
        nameText = UIBuilder.Text(UIBuilder.Child("Name", stage, middle, center + new Vector2(0f, -350f), new Vector2(980f, 90f)),
            model, "", 62, TextAnchor.MiddleCenter);
        nameText.color = Color.white;
        statusText = UIBuilder.Text(UIBuilder.Child("Status", stage, middle, center + new Vector2(0f, -440f), new Vector2(900f, 70f)),
            model, "", 48, TextAnchor.MiddleCenter);

        summary = UIBuilder.Child("Summary", transform, middle, Vector2.zero, new Vector2(1040f, 900f));

        flash = UIBuilder.Image(UIBuilder.Child("Flash", transform, Vector2.zero, Vector2.one), null, new Color(1f, 1f, 1f, 0f), false);

        hintText = UIBuilder.Text(UIBuilder.Child("Hint", transform, new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(900f, 60f)),
            model, "", 36, TextAnchor.MiddleCenter);
        hintText.color = new Color(1f, 1f, 1f, 0.6f);
        counterText = UIBuilder.Text(UIBuilder.Child("Counter", transform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(400f, 70f)),
            model, "", 48, TextAnchor.MiddleCenter);
        counterText.color = Color.white;
        RectTransform skipRect = UIBuilder.Child("Skip", transform, new Vector2(1f, 1f), new Vector2(-160f, -170f), new Vector2(240f, 90f));
        UIBuilder.Button(skipRect, "Passer", 40, () =>
        {
            skip = true;
            tapped = true;
        });
        skipButton = skipRect.gameObject;
    }

    // ---- sequence

    private IEnumerator Run(List<DrawAPI> draws, Action onDone)
    {
        int best = draws.Max(draw => CharacterRank.Tier(draw.rank));
        skip = false;
        ClearSummary();
        yield return Charge(best, draws.Count > 1);

        for (int i = 0; i < draws.Count && !skip; i++)
        {
            yield return Reveal(draws[i], i, draws.Count);
        }
        if (draws.Count > 1)
        {
            yield return Summary(draws, best);
        }
        ClearSummary();
        gameObject.SetActive(false);
        onDone?.Invoke();
    }

    /// <summary>
    /// The portal before the characters come out. It climbs from the lowest rank to the best rank of the draws:
    /// each step is a pulse of the color of its rank, stronger than the one before, and the last steps let the
    /// user wait. So a portal that stays grey gives little, and one that keeps climbing announces a rare character.
    /// </summary>
    private IEnumerator Charge(int best, bool several)
    {
        ShowCard(false);
        portal.gameObject.SetActive(true);
        SetRays(0);
        tapped = false;
        colorTier = 0;
        spin = 50f;
        glowAlpha = 0.45f;
        coreAlpha = 1f;
        rayAlpha = 0f;
        shake = 0f;
        shakeFloor = 0f;
        flashAlpha = 0f;
        counterText.text = "";
        hintText.text = "";
        skipButton.SetActive(several);
        SoundManager.Play(Sfx.PortalOpen);

        // the portal opens
        for (float t = 0f; t < 0.9f && !tapped; t += Time.unscaledDeltaTime)
        {
            float progress = t / 0.9f;
            portalScale = Mathf.Lerp(0.35f, 1f, EaseOutBack(progress));
            spin = Mathf.Lerp(50f, 180f, progress);
            Gather(40f * Time.unscaledDeltaTime);
            yield return null;
        }
        portalScale = 1f;

        for (int tier = 1; tier <= best && !tapped; tier++)
        {
            // it trembles before each step, longer for the highest ranks: will it climb again ?
            float wait = 0.3f + 0.1f * tier + (tier >= 4 ? 0.4f : 0f);
            shakeFloor = 1.5f * tier;
            yield return Hold(wait, 30f + 15f * tier);
            if (tapped) break;

            colorTier = tier;
            // one tone higher at each rank: the ear follows the climb too
            SoundManager.Play(Sfx.SummonStep, Mathf.Pow(2f, (tier - 1) * 2f / 12f));
            spin = 180f + 90f * tier;
            glowAlpha = Mathf.Min(1f, 0.45f + 0.1f * tier);
            SetRays(tier >= 3 ? 6 + 6 * (tier - 3) : 0);
            rayAlpha = tier >= 3 ? 0.14f + 0.07f * (tier - 3) : 0f;
            flashAlpha = tier >= 4 ? 0.75f : 0.12f * tier;
            shake = 7f * tier;
            Burst(8 + 7 * tier, 1f + 0.15f * tier);
            yield return Pulse(1.12f + 0.04f * tier, 0.22f);
        }

        if (!tapped)
        {
            shakeFloor = 1.5f * best;
            yield return Hold(0.4f + 0.1f * best, 30f + 15f * best);
        }
        if (!tapped && best >= CharacterRank.TIER_MAX)
        {
            // the best rank: the portal cannot hold what is coming
            for (int i = 0; i < 3 && !tapped; i++)
            {
                flashAlpha = 0.9f;
                shake = 45f;
                Burst(45, 1.8f);
                SoundManager.Play(Sfx.SummonStep, 2f + 0.12f * i);
                yield return Pulse(1.35f, 0.3f);
            }
        }

        // it bursts open
        colorTier = best;
        flashAlpha = 1f;
        shakeFloor = 0f;
        shake = 10f + 6f * best;
        Burst(24 + 12 * best, 1.2f + 0.15f * best);
        SoundManager.Play(Sfx.SummonBurst, 1f, 0.5f + 0.08f * best);
    }

    /// <summary>The jingle of a character coming out: a few notes for the low ranks, a whole fanfare for the SSS.</summary>
    private static Sfx RevealSound(int tier)
    {
        if (tier >= CharacterRank.TIER_MAX) return Sfx.RevealLegend;
        if (tier >= 4) return Sfx.RevealEpic;
        if (tier >= 2) return Sfx.RevealRare;
        return Sfx.RevealCommon;
    }

    /// <summary>A character comes out of the portal; the step ends on a touch.</summary>
    private IEnumerator Reveal(DrawAPI draw, int index, int count)
    {
        int tier = CharacterRank.Tier(draw.rank);
        colorTier = tier;
        portalScale = 1f;
        coreAlpha = 0f;
        glowAlpha = 0.35f + 0.09f * tier;
        spin = 60f + 50f * tier;
        SetRays(tier >= 3 ? 6 + 6 * (tier - 3) : 0);
        rayAlpha = tier >= 3 ? 0.14f + 0.07f * (tier - 3) : 0f;
        shakeFloor = 0f;
        counterText.text = count > 1 ? (index + 1) + " / " + count : "";
        skipButton.SetActive(count > 1);
        hintText.text = "";

        ShowCard(true);
        portrait.sprite = CharacterArt.Portrait(draw.num);
        portrait.enabled = portrait.sprite != null;
        portrait.rectTransform.localScale = Vector3.zero;
        rankText.text = draw.rank;
        rankText.rectTransform.localScale = Vector3.zero;
        nameText.text = draw.name;
        statusText.text = draw.isNew ? "NOUVEAU !" : "Doublon  x" + draw.copies;
        statusText.color = draw.isNew ? newColor : duplicateColor;
        statusText.rectTransform.localScale = Vector3.one;
        SetAlpha(nameText, 0f);
        SetAlpha(statusText, 0f);
        tapped = false;

        if (tier >= 4)
        {
            // the highest ranks first come as a shadow, growing in the light, before they are shown
            portrait.color = Color.black;
            SoundManager.Play(Sfx.PortalOpen, 1.5f);
            float rise = 0.55f + 0.3f * (tier - 4);
            shakeFloor = 4f * (tier - 3);
            for (float t = 0f; t < rise; t += Time.unscaledDeltaTime)
            {
                portrait.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, t / rise);
                glowAlpha = Mathf.Lerp(0.4f, 1f, t / rise);
                Gather((60f + 40f * (tier - 4)) * Time.unscaledDeltaTime);
                yield return null;
            }
            shakeFloor = 0f;
            flashAlpha = 1f;
            shake = 26f + 12f * (tier - 4);
            Burst(45 + 25 * (tier - 4), 1.6f);
            SoundManager.Play(Sfx.SummonBurst, 1.2f, 0.7f);
            SoundManager.Play(RevealSound(tier));
            if (tier >= CharacterRank.TIER_MAX)
            {
                yield return Pulse(1.3f, 0.18f);
                flashAlpha = 1f;
                shake = 50f;
                Burst(70, 2f);
            }
            portrait.color = Color.white;
            portrait.rectTransform.localScale = Vector3.one;
        }
        else
        {
            portrait.color = Color.white;
            flashAlpha = Mathf.Max(flashAlpha, 0.12f + 0.12f * tier);
            shake = Mathf.Max(shake, 4f * tier);
            Burst(8 + 8 * tier, 1f);
            SoundManager.Play(RevealSound(tier));
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                portrait.rectTransform.localScale = Vector3.one * EaseOutBack(t / 0.3f);
                yield return null;
            }
            portrait.rectTransform.localScale = Vector3.one;
        }

        // the rank is stamped above the character
        for (float t = 0f; t < 0.22f; t += Time.unscaledDeltaTime)
        {
            float progress = t / 0.22f;
            rankText.rectTransform.localScale = Vector3.one * Mathf.Lerp(3f, 1f, progress * progress);
            yield return null;
        }
        rankText.rectTransform.localScale = Vector3.one;
        SoundManager.Play(Sfx.RankStamp);
        shake = Mathf.Max(shake, 3f + 4f * tier);
        Burst(4 + 4 * tier, 0.8f);

        for (float t = 0f; t < 0.2f; t += Time.unscaledDeltaTime)
        {
            SetAlpha(nameText, t / 0.2f);
            SetAlpha(statusText, t / 0.2f);
            yield return null;
        }
        SetAlpha(nameText, 1f);
        SetAlpha(statusText, 1f);
        hintText.text = "Toucher pour continuer";

        // the touches made during the entrance do not count
        tapped = false;
        float sparkle = 0f;
        while (!tapped && !skip)
        {
            if (draw.isNew)
            {
                statusText.rectTransform.localScale = Vector3.one * (1f + 0.07f * Mathf.Sin(clock * 7f));
            }
            // the highest ranks keep shining
            sparkle += Mathf.Max(0, tier - 3) * 10f * Time.unscaledDeltaTime;
            while (sparkle >= 1f)
            {
                sparkle -= 1f;
                Burst(1, 0.7f);
            }
            yield return null;
        }
    }

    /// <summary>Every character of the draws, together; the step ends on a touch.</summary>
    private IEnumerator Summary(List<DrawAPI> draws, int best)
    {
        ShowCard(false);
        SetRays(0);
        colorTier = best;
        coreAlpha = 0f;
        glowAlpha = 0.3f;
        rayAlpha = 0f;
        spin = 40f;
        shakeFloor = 0f;
        counterText.text = "Résultat";
        hintText.text = "Toucher pour continuer";
        skipButton.SetActive(false);

        const int columns = 5;
        int rows = Mathf.CeilToInt(draws.Count / (float)columns);
        Vector2 cellSize = new Vector2(196f, 300f);
        Vector2 spacing = new Vector2(208f, 320f);
        for (int i = 0; i < draws.Count; i++)
        {
            DrawAPI draw = draws[i];
            int column = i % columns;
            int row = i / columns;
            int inRow = Mathf.Min(columns, draws.Count - row * columns);
            Vector2 position = new Vector2((column - (inRow - 1) / 2f) * spacing.x, ((rows - 1) / 2f - row) * spacing.y);
            RectTransform cell = UIBuilder.Child("Draw" + i, summary, new Vector2(0.5f, 0.5f), position, cellSize);
            Color rankColor = CharacterRank.Color(draw.rank);
            UIBuilder.Image(cell, null, new Color(rankColor.r, rankColor.g, rankColor.b, 0.4f), false);
            Image picture = UIBuilder.Image(UIBuilder.Child("Portrait", cell, new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.98f)),
                CharacterArt.Portrait(draw.num), Color.white, false);
            picture.preserveAspect = true;
            picture.enabled = picture.sprite != null;
            Text rank = UIBuilder.Text(UIBuilder.Child("Rank", cell, new Vector2(0.05f, 0.02f), new Vector2(0.5f, 0.24f)),
                UIBuilder.ModelText, draw.rank, 44, TextAnchor.MiddleLeft);
            rank.color = rankColor;
            Text status = UIBuilder.Text(UIBuilder.Child("Status", cell, new Vector2(0.5f, 0.02f), new Vector2(0.96f, 0.24f)),
                UIBuilder.ModelText, draw.isNew ? "NEW" : "x" + draw.copies, 34, TextAnchor.MiddleRight);
            status.color = draw.isNew ? newColor : duplicateColor;

            // they pop in one after the other
            SoundManager.Play(Sfx.Pop, 1f + 0.06f * i);
            cell.localScale = Vector3.zero;
            for (float t = 0f; t < 0.08f && !tapped; t += Time.unscaledDeltaTime)
            {
                cell.localScale = Vector3.one * (t / 0.08f);
                yield return null;
            }
            cell.localScale = Vector3.one;
        }

        tapped = false;
        while (!tapped)
        {
            yield return null;
        }
    }

    // ---- small steps of the sequence

    /// <summary>Waits while the portal draws sparks in; ends early on a touch.</summary>
    private IEnumerator Hold(float duration, float sparksPerSecond)
    {
        for (float t = 0f; t < duration && !tapped; t += Time.unscaledDeltaTime)
        {
            Gather(sparksPerSecond * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    /// <summary>The portal swells to a scale and comes back.</summary>
    private IEnumerator Pulse(float scale, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            portalScale = Mathf.Lerp(scale, 1f, t / duration);
            yield return null;
        }
        portalScale = 1f;
    }

    private void ShowCard(bool shown)
    {
        portrait.gameObject.SetActive(shown);
        rankText.gameObject.SetActive(shown);
        nameText.gameObject.SetActive(shown);
        statusText.gameObject.SetActive(shown);
    }

    private void ClearSummary()
    {
        for (int i = summary.childCount - 1; i >= 0; i--)
        {
            Destroy(summary.GetChild(i).gameObject);
        }
    }

    /// <summary>Shows this many rays, spread around the portal.</summary>
    private void SetRays(int count)
    {
        count = Mathf.Min(count, maxRays);
        for (int i = 0; i < rayImages.Count; i++)
        {
            rayImages[i].enabled = i < count;
            if (i < count) rayImages[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, i * 360f / count);
        }
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = Mathf.Clamp01(alpha);
        graphic.color = color;
    }

    private static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t) - 1f;
        return 1f + 2.70158f * t * t * t + 1.70158f * t * t;
    }

    // ---- particles

    private float gatherDebt;

    /// <summary>Sparks pulled into the portal; "count" may be a fraction, for a rate over the frames.</summary>
    private void Gather(float count)
    {
        gatherDebt += count;
        while (gatherDebt >= 1f)
        {
            gatherDebt -= 1f;
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            float distance = UnityEngine.Random.Range(420f, 680f);
            float duration = UnityEngine.Random.Range(0.35f, 0.6f);
            Spawn(direction * distance, -direction * (distance / duration), 0f, duration, UnityEngine.Random.Range(14f, 30f));
        }
    }

    /// <summary>Sparks thrown out of the portal.</summary>
    private void Burst(int count, float strength)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            Spawn(direction * UnityEngine.Random.Range(0f, 60f), direction * UnityEngine.Random.Range(350f, 1100f) * strength, 1.6f,
                UnityEngine.Random.Range(0.5f, 1.1f), UnityEngine.Random.Range(16f, 42f));
        }
    }

    private void Spawn(Vector2 position, Vector2 velocity, float drag, float duration, float size)
    {
        Particle particle = particles.FirstOrDefault(p => !p.alive);
        if (particle == null)
        {
            if (particles.Count >= maxParticles) return;
            RectTransform rect = UIBuilder.Child("Spark", particleLayer, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.one);
            particle = new Particle { rect = rect, image = UIBuilder.Image(rect, FxSprites.Soft, Color.white, false) };
            particles.Add(particle);
        }
        particle.alive = true;
        particle.position = position;
        particle.velocity = velocity;
        particle.drag = drag;
        particle.age = 0f;
        particle.duration = duration;
        particle.size = size;
        particle.image.enabled = true;
    }

    // ---- animation

    void Update()
    {
        float delta = Time.unscaledDeltaTime;
        clock += delta;
        Color color = CharacterRank.AnimatedColor(colorTier, clock);

        portal.localScale = Vector3.one * portalScale;
        ringA.rectTransform.Rotate(0f, 0f, spin * delta);
        ringB.rectTransform.Rotate(0f, 0f, -spin * 1.4f * delta);
        rays.Rotate(0f, 0f, spin * 0.12f * delta);
        ringA.color = Color.Lerp(color, Color.white, 0.25f);
        ringB.color = Color.Lerp(color, Color.white, 0.55f);
        glow.color = new Color(color.r, color.g, color.b, glowAlpha);
        glow.rectTransform.localScale = Vector3.one * (1f + 0.06f * Mathf.Sin(clock * 8f));
        core.color = new Color(1f, 1f, 1f, coreAlpha);
        Color rayColor = new Color(color.r, color.g, color.b, rayAlpha);
        foreach (Image ray in rayImages)
        {
            if (ray.enabled) ray.color = rayColor;
        }
        if (rankText.gameObject.activeSelf) rankText.color = color;

        shake = Mathf.MoveTowards(shake, shakeFloor, 60f * delta);
        stage.anchoredPosition = shake > 0.01f ? UnityEngine.Random.insideUnitCircle * shake : Vector2.zero;
        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, 2.2f * delta);
        flash.color = new Color(1f, 1f, 1f, flashAlpha);

        Color sparkColor = Color.Lerp(color, Color.white, 0.5f);
        foreach (Particle particle in particles)
        {
            if (!particle.alive) continue;
            particle.age += delta;
            if (particle.age >= particle.duration)
            {
                particle.alive = false;
                particle.image.enabled = false;
                continue;
            }
            float life = particle.age / particle.duration;
            particle.velocity *= Mathf.Max(0f, 1f - particle.drag * delta);
            particle.position += particle.velocity * delta;
            particle.rect.anchoredPosition = particle.position;
            particle.rect.sizeDelta = Vector2.one * particle.size * (1f - 0.5f * life);
            particle.image.color = new Color(sparkColor.r, sparkColor.g, sparkColor.b, 1f - life);
        }
    }
}
