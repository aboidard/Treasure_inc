using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>What a list of characters can be sorted by.</summary>
public enum CharacterSort { Number, Rank, Level, Power, Name }

/// <summary>What the sorts and the filters need to know about a character, whether the user has it or not.</summary>
public struct CharacterEntry
{
    public int num;
    public string name;
    public string rank;
    public int level;
    /// <summary>The sum of its stats, see GameCharacter.Power.</summary>
    public int power;
    public bool available;

    public static CharacterEntry Of(GameCharacter character)
    {
        return new CharacterEntry
        {
            num = character.num,
            name = character.name,
            rank = character.rank,
            level = character.level,
            power = character.Power,
            available = character.IsAvailable,
        };
    }
}

/// <summary>
/// How a screen lists the characters: its sort and its filters (by rank, the available ones, the favorites).
/// Each screen has its own options (see For), kept in the preferences so they are found again at the next start.
/// The favorites always come first, whatever the sort. CharacterFilterBar shows these options and changes them.
/// </summary>
public class CharacterListOptions
{
    private const string prefsPrefix = "characterList.";
    private static readonly Dictionary<string, CharacterListOptions> byScreen = new Dictionary<string, CharacterListOptions>();

    private static readonly KeyValuePair<CharacterSort, string>[] sortNames =
    {
        new KeyValuePair<CharacterSort, string>(CharacterSort.Number, "Numéro"),
        new KeyValuePair<CharacterSort, string>(CharacterSort.Rank, "Rang"),
        new KeyValuePair<CharacterSort, string>(CharacterSort.Level, "Niveau"),
        new KeyValuePair<CharacterSort, string>(CharacterSort.Power, "Puissance"),
        new KeyValuePair<CharacterSort, string>(CharacterSort.Name, "Nom"),
    };

    private readonly string screen;
    private readonly HashSet<string> hiddenRanks = new HashSet<string>();

    public CharacterSort Sort { get; private set; }
    public bool Descending { get; private set; }
    public bool FavoritesOnly { get; private set; }
    public bool AvailableOnly { get; private set; }

    private CharacterListOptions(string screen, CharacterSort sort, bool descending)
    {
        this.screen = screen;
        Sort = sort;
        Descending = descending;
    }

    /// <summary>The options of a screen, read from the preferences the first time.</summary>
    /// <param name="screen">a name of its own for each list, e.g. "team"</param>
    /// <param name="sort">the sort of the list until the user chooses another one</param>
    public static CharacterListOptions For(string screen, CharacterSort sort = CharacterSort.Number, bool descending = false)
    {
        if (!byScreen.TryGetValue(screen, out CharacterListOptions options))
        {
            options = new CharacterListOptions(screen, sort, descending);
            options.Load();
            byScreen[screen] = options;
        }
        return options;
    }

    public string SortName => sortNames.First(s => s.Key == Sort).Value;

    /// <summary>Whether some characters may be left out of the list.</summary>
    public bool IsFiltering => FavoritesOnly || AvailableOnly || hiddenRanks.Count > 0;

    public bool ShowsRank(string rank)
    {
        return !hiddenRanks.Contains(rank);
    }

    /// <summary>The next sort, in its usual order: the best first, except for the names and the numbers.</summary>
    public void NextSort()
    {
        int index = Array.FindIndex(sortNames, s => s.Key == Sort);
        Sort = sortNames[(index + 1) % sortNames.Length].Key;
        Descending = Sort == CharacterSort.Rank || Sort == CharacterSort.Level || Sort == CharacterSort.Power;
        Save();
    }

    public void ToggleDirection()
    {
        Descending = !Descending;
        Save();
    }

    public void ToggleFavoritesOnly()
    {
        FavoritesOnly = !FavoritesOnly;
        Save();
    }

    public void ToggleAvailableOnly()
    {
        AvailableOnly = !AvailableOnly;
        Save();
    }

    public void ToggleRank(string rank)
    {
        if (!hiddenRanks.Remove(rank)) hiddenRanks.Add(rank);
        Save();
    }

    /// <summary>The characters of the user this screen shows, in its order.</summary>
    public List<GameCharacter> Apply(IEnumerable<GameCharacter> characters)
    {
        return Apply(characters, CharacterEntry.Of);
    }

    /// <summary>The elements this screen shows, in its order; describe says which character an element is.</summary>
    public List<T> Apply<T>(IEnumerable<T> source, Func<T, CharacterEntry> describe)
    {
        var kept = source.Select(element => new KeyValuePair<T, CharacterEntry>(element, describe(element)))
            .Where(pair => Shows(pair.Value))
            .ToList();
        kept.Sort((a, b) => Compare(a.Value, b.Value));
        return kept.Select(pair => pair.Key).ToList();
    }

    private bool Shows(CharacterEntry entry)
    {
        if (FavoritesOnly && !CharacterFavorites.Contains(entry.num)) return false;
        if (AvailableOnly && !entry.available) return false;
        return ShowsRank(entry.rank);
    }

    private int Compare(CharacterEntry a, CharacterEntry b)
    {
        int favorites = CharacterFavorites.Contains(b.num).CompareTo(CharacterFavorites.Contains(a.num));
        if (favorites != 0) return favorites;

        int order;
        switch (Sort)
        {
            case CharacterSort.Rank: order = CharacterRank.Tier(a.rank).CompareTo(CharacterRank.Tier(b.rank)); break;
            case CharacterSort.Level: order = a.level.CompareTo(b.level); break;
            case CharacterSort.Power: order = a.power.CompareTo(b.power); break;
            case CharacterSort.Name: order = string.Compare(a.name, b.name, StringComparison.CurrentCultureIgnoreCase); break;
            default: order = a.num.CompareTo(b.num); break;
        }
        if (Descending) order = -order;
        if (order != 0) return order;

        // equal characters: the best rank first, then always the same order
        int rank = CharacterRank.Tier(b.rank).CompareTo(CharacterRank.Tier(a.rank));
        return rank != 0 ? rank : a.num.CompareTo(b.num);
    }

    // "sort;descending;favoritesOnly;availableOnly;hidden ranks", e.g. "Level;1;0;1;D,C"
    private void Save()
    {
        PlayerPrefs.SetString(prefsPrefix + screen, string.Join(";", Sort.ToString(), Descending ? "1" : "0",
            FavoritesOnly ? "1" : "0", AvailableOnly ? "1" : "0", string.Join(",", hiddenRanks.OrderBy(r => r))));
        PlayerPrefs.Save();
    }

    private void Load()
    {
        string[] parts = PlayerPrefs.GetString(prefsPrefix + screen, "").Split(';');
        if (parts.Length < 5 || !Enum.TryParse(parts[0], out CharacterSort sort)) return;
        Sort = sort;
        Descending = parts[1] == "1";
        FavoritesOnly = parts[2] == "1";
        AvailableOnly = parts[3] == "1";
        foreach (string rank in parts[4].Split(','))
        {
            if (rank.Length > 0) hiddenRanks.Add(rank);
        }
    }
}

/// <summary>
/// The favorite characters of the user: they come first in every list, which can also show them alone.
/// They are kept in the preferences of the device, by user: the server does not know them.
/// </summary>
public static class CharacterFavorites
{
    private const string prefsPrefix = "favoriteCharacters.";

    private static string loadedFor;
    private static readonly HashSet<int> favorites = new HashSet<int>();

    public static bool Contains(int num)
    {
        Load();
        return favorites.Contains(num);
    }

    /// <summary>Makes the character a favorite, or no longer one; answers whether it is one now.</summary>
    public static bool Toggle(int num)
    {
        Load();
        bool added = favorites.Add(num);
        if (!added) favorites.Remove(num);
        PlayerPrefs.SetString(prefsPrefix + loadedFor, string.Join(",", favorites.OrderBy(n => n)));
        PlayerPrefs.Save();
        return added;
    }

    private static void Load()
    {
        string user = NetworkManager.Instance.publicKey ?? "";
        if (loadedFor == user) return;
        loadedFor = user;
        favorites.Clear();
        foreach (string part in PlayerPrefs.GetString(prefsPrefix + user, "").Split(','))
        {
            if (int.TryParse(part, out int num)) favorites.Add(num);
        }
    }
}
