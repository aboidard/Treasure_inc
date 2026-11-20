using System.Collections.Generic;

/// <summary>
/// The gift box of the user as the API answers it: the gifts sent by the administrators or given by the game,
/// waiting to be taken. The server gives the rewards: the client only shows them.
/// </summary>
[System.Serializable]
public class GiftsAPI
{
    /// <summary>How many gifts wait in the box (the list may hold only the latest ones).</summary>
    public int count { get; set; }
    /// <summary>The latest first.</summary>
    public List<GiftAPI> gifts { get; set; }
}

[System.Serializable]
public class GiftAPI
{
    public int id { get; set; }
    /// <summary>What the gift is for, written by whoever sent it.</summary>
    public string title { get; set; }
    public GiftRewardAPI reward { get; set; }
    public string createdAt { get; set; }
    /// <summary>When the gift can no longer be taken (ISO-8601); null when it never expires.</summary>
    public string expiresAt { get; set; }
}

[System.Serializable]
public class GiftRewardAPI
{
    public int money { get; set; }
    public int gems { get; set; }
    /// <summary>How many items the gift generates when it is taken.</summary>
    public int items { get; set; }
    /// <summary>The rarity of these items ("common"... "legendary"); null when each one is drawn.</summary>
    public string rarity { get; set; }
}

/// <summary>The answer to a gift taken: what it gave, and the state of the user afterwards.</summary>
[System.Serializable]
public class GiftClaimResponse
{
    public int gift { get; set; }
    public string title { get; set; }
    /// <summary>The balances of the user afterwards.</summary>
    public int money { get; set; }
    public int gems { get; set; }
    public GiftClaimRewardAPI reward { get; set; }
    /// <summary>What is left in the box.</summary>
    public GiftsAPI gifts { get; set; }
}

[System.Serializable]
public class GiftClaimRewardAPI
{
    public int money { get; set; }
    public int gems { get; set; }
    public List<ItemAPI> items { get; set; }
}

public static class Gift
{
    /// <summary>"1 000 gemmes + 5 000 pièces + 3 objets épiques".</summary>
    public static string Reward(GiftRewardAPI reward)
    {
        if (reward == null) return "";
        List<string> parts = new List<string>();
        if (reward.gems > 0) parts.Add(Gems.Amount(reward.gems));
        if (reward.money > 0) parts.Add(UIBuilder.Number(reward.money) + " pièces");
        if (reward.items > 0)
        {
            string items = reward.items + (reward.items > 1 ? " objets" : " objet");
            string rarity = RarityName(reward.rarity, reward.items > 1);
            parts.Add(rarity == null ? items : items + " " + rarity);
        }
        return string.Join(" + ", parts);
    }

    /// <summary>"Expire dans 3 j 4 h", or null when the gift never expires.</summary>
    public static string Expiry(GiftAPI gift)
    {
        if (string.IsNullOrEmpty(gift.expiresAt)) return null;
        if (!System.DateTime.TryParse(gift.expiresAt, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                out System.DateTime expiresAt))
        {
            return null;
        }
        double left = (expiresAt - System.DateTime.UtcNow).TotalSeconds;
        return left > 0 ? "Expire dans " + Quest.Duration((float)left) : "Expiré";
    }

    private static string RarityName(string rarity, bool plural)
    {
        string s = plural ? "s" : "";
        switch (rarity)
        {
            case "common": return "commun" + s;
            case "uncommon": return "peu commun" + s;
            case "rare": return "rare" + s;
            case "epic": return "épique" + s;
            case "legendary": return "légendaire" + s;
            default: return null;
        }
    }
}
