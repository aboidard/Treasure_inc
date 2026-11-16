using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The portraits of the characters, by their number. The project only has the art of the three characters every
/// user starts with: until the others get their own, each of them is one of these three with other colors
/// (see variants). Giving a character its real portrait: return its sprite from Portrait instead.
/// </summary>
public static class CharacterArt
{
    private struct Variant
    {
        public float hue;        // added to the hue of the colors, 0..1
        public float saturation; // multiplies their saturation
        public float value;      // multiplies their brightness

        public Variant(float hue, float saturation, float value)
        {
            this.hue = hue;
            this.saturation = saturation;
            this.value = value;
        }
    }

    // by character number, from the first character that has no art of its own (number = bases + 1 + index)
    private static readonly Variant[] variants =
    {
        new Variant(0.33f, 1f, 1f),
        new Variant(0.58f, 1f, 1f),
        new Variant(0.12f, 1.1f, 1.05f),
        new Variant(0.75f, 1f, 1f),
        new Variant(0.45f, 0.8f, 1.1f),
        new Variant(0.9f, 1f, 1f),
        new Variant(0.2f, 1.2f, 0.95f),
        new Variant(0.66f, 0.7f, 1.15f),
        new Variant(0.05f, 0.5f, 1.2f),
        new Variant(0.5f, 1.2f, 1f),
        new Variant(0.83f, 0.8f, 1.1f),
        new Variant(0.27f, 0.6f, 1.2f),
        new Variant(0.62f, 1.3f, 0.9f),
        new Variant(0.4f, 1.3f, 1.1f),
        new Variant(0.15f, 1.4f, 1.2f),
        new Variant(0.7f, 1.4f, 1.2f),
        new Variant(0.95f, 1.4f, 1.25f),
    };

    private static List<Sprite> bases = new List<Sprite>();
    private static readonly Dictionary<int, Sprite> portraits = new Dictionary<int, Sprite>();

    /// <summary>The portraits of the characters 1, 2, 3...: the art the project has.</summary>
    public static void SetBases(List<Sprite> sprites)
    {
        bases = sprites ?? new List<Sprite>();
        portraits.Clear();
    }

    /// <summary>The portrait of a character; null until the art is known (see SetBases).</summary>
    public static Sprite Portrait(int num)
    {
        if (bases.Count == 0 || num < 1) return null;
        if (num <= bases.Count) return bases[num - 1];
        if (portraits.TryGetValue(num, out Sprite known) && known != null) return known;

        int index = num - bases.Count - 1;
        Sprite source = bases[index % bases.Count];
        Sprite portrait = Recolor(source, variants[index % variants.Length]);
        portraits[num] = portrait;
        return portrait;
    }

    /// <summary>A copy of the sprite with other colors; the skin tones are kept.</summary>
    private static Sprite Recolor(Sprite source, Variant variant)
    {
        if (source == null) return null;
        Texture2D texture = source.texture;
        Rect area = source.textureRect;
        int width = Mathf.RoundToInt(area.width);
        int height = Mathf.RoundToInt(area.height);

        // the texture of the sprite is not readable: it is drawn in a render texture, which is
        RenderTexture target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previous = RenderTexture.active;
        Graphics.Blit(texture, target);
        RenderTexture.active = target;
        Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(area.x, area.y, width, height), 0, 0);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);

        Color[] pixels = copy.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            if (pixel.a <= 0f) continue;
            Color.RGBToHSV(pixel, out float h, out float s, out float v);
            bool skin = h > 0.03f && h < 0.13f && s < 0.6f && v > 0.45f;
            if (skin) continue;
            Color changed = Color.HSVToRGB(Mathf.Repeat(h + variant.hue, 1f), Mathf.Clamp01(s * variant.saturation),
                Mathf.Clamp01(v * variant.value));
            changed.a = pixel.a;
            pixels[i] = changed;
        }
        copy.SetPixels(pixels);
        copy.filterMode = FilterMode.Point;
        copy.wrapMode = TextureWrapMode.Clamp;
        copy.Apply();
        return Sprite.Create(copy, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit);
    }
}
