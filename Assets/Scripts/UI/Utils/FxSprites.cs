using System;
using UnityEngine;

/// <summary>
/// The white shapes of the effects drawn by code (portal, summoning), to be tinted by their Image: they are
/// computed once instead of being imported, so the effects need no asset.
/// </summary>
public static class FxSprites
{
    private static Sprite soft, ringA, ringB, ray;

    /// <summary>A disc fading towards its edge: glows and sparks.</summary>
    public static Sprite Soft
    {
        get
        {
            if (soft == null) soft = Build(64, 64, new Vector2(0.5f, 0.5f), (u, v) =>
            {
                float a = Mathf.Clamp01(1f - Mathf.Sqrt(u * u + v * v));
                return a * a;
            });
            return soft;
        }
    }

    /// <summary>A ring cut in 6 arcs, whose rotation can be seen.</summary>
    public static Sprite RingA
    {
        get
        {
            if (ringA == null) ringA = Build(256, 256, new Vector2(0.5f, 0.5f), (u, v) => Ring(u, v, 0.86f, 0.07f, 6));
            return ringA;
        }
    }

    /// <summary>A thinner ring cut in 10 arcs.</summary>
    public static Sprite RingB
    {
        get
        {
            if (ringB == null) ringB = Build(256, 256, new Vector2(0.5f, 0.5f), (u, v) => Ring(u, v, 0.88f, 0.045f, 10));
            return ringB;
        }
    }

    /// <summary>A ray of light widening and fading from its pivot, at the bottom, to its top.</summary>
    public static Sprite Ray
    {
        get
        {
            if (ray == null) ray = Build(32, 128, new Vector2(0.5f, 0f), (u, v) =>
            {
                float along = (v + 1f) / 2f;
                float halfWidth = Mathf.Max(0.02f, along);
                float across = Mathf.Abs(u);
                return across >= halfWidth ? 0f : (1f - across / halfWidth) * (1f - along);
            });
            return ray;
        }
    }

    private static float Ring(float u, float v, float radius, float thickness, int arcs)
    {
        float distance = Mathf.Sqrt(u * u + v * v);
        float band = Mathf.Clamp01(1f - Mathf.Abs(distance - radius) / thickness);
        float arc = Mathf.Sqrt(Mathf.Abs(Mathf.Sin(Mathf.Atan2(v, u) * arcs / 2f)));
        return band * (0.25f + 0.75f * arc);
    }

    /// <param name="alpha">the opacity of a point, whose coordinates go from -1 to 1 on each axis</param>
    private static Sprite Build(int width, int height, Vector2 pivot, Func<float, float, float> alpha)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width * 2f - 1f;
                float v = (y + 0.5f) / height * 2f - 1f;
                pixels[y * width + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha(u, v)));
            }
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, 100f);
    }
}
