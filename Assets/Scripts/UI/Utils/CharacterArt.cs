using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The portraits of the characters, by their number. The three characters every user starts with take theirs from
/// the TeamPanel of the scene (see SetBases); the others from Resources/CharacterPortraits (see CharacterPortraits).
/// Giving a new character its portrait: add the front frame of its sprite sheet at the end of that asset.
/// </summary>
public static class CharacterArt
{
    private const string PortraitsResource = "CharacterPortraits";

    private static List<Sprite> bases = new List<Sprite>();
    private static Sprite[] others;

    /// <summary>The portraits of the characters 1, 2, 3...: the art of the scene.</summary>
    public static void SetBases(List<Sprite> sprites)
    {
        bases = sprites ?? new List<Sprite>();
    }

    /// <summary>The portrait of a character; null until the art is known (see SetBases).</summary>
    public static Sprite Portrait(int num)
    {
        if (bases.Count == 0 || num < 1) return null;
        if (num <= bases.Count) return bases[num - 1];

        int index = num - bases.Count - 1;
        Sprite[] portraits = Others();
        if (index < portraits.Length && portraits[index] != null) return portraits[index];
        // a character newer than the art of the client: one of the bases rather than nothing
        return bases[index % bases.Count];
    }

    private static Sprite[] Others()
    {
        if (others == null)
        {
            CharacterPortraits asset = Resources.Load<CharacterPortraits>(PortraitsResource);
            if (asset == null) Debug.LogWarning("Resources/" + PortraitsResource + " not found: no portrait for the recruited characters");
            others = asset != null && asset.portraits != null ? asset.portraits : new Sprite[0];
        }
        return others;
    }
}
