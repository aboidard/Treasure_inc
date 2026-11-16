using System.Collections.Generic;

/// <summary>
/// The quests of the user as the API answers them: a daily board and a weekly one. The server draws the quests,
/// counts their progress and gives their rewards; the client only writes their sentence (see Quest.Describe).
/// </summary>
[System.Serializable]
public class QuestsAPI
{
    /// <summary>How many completed quests wait for their reward to be taken.</summary>
    public int claimable { get; set; }
    public QuestBoardAPI daily { get; set; }
    public QuestBoardAPI weekly { get; set; }
}

[System.Serializable]
public class QuestBoardAPI
{
    /// <summary>The day ("2026-11-16") or the week ("2026-W47") of the board.</summary>
    public string period { get; set; }
    /// <summary>Seconds left, when the answer was made, before the board is replaced by a new one.</summary>
    public long resetIn { get; set; }
    public List<QuestAPI> quests { get; set; }
}

[System.Serializable]
public class QuestAPI
{
    public string id { get; set; }
    /// <summary>What the quest counts: see Quest.Describe.</summary>
    public string metric { get; set; }
    public int goal { get; set; }
    /// <summary>From 0 to goal.</summary>
    public int progress { get; set; }
    public bool completed { get; set; }
    /// <summary>True once the reward was taken.</summary>
    public bool claimed { get; set; }
    public QuestRewardAPI reward { get; set; }

    public bool IsClaimable => completed && !claimed;
}

[System.Serializable]
public class QuestRewardAPI
{
    public int money { get; set; }
    /// <summary>How many items the quest gives.</summary>
    public int items { get; set; }
}

/// <summary>The answer to a claimed quest: what it gave, and the state of the user afterwards.</summary>
[System.Serializable]
public class QuestClaimResponse
{
    public string quest { get; set; }
    /// <summary>The balance of the user afterwards.</summary>
    public int money { get; set; }
    public QuestClaimRewardAPI reward { get; set; }
    public QuestsAPI quests { get; set; }
}

[System.Serializable]
public class QuestClaimRewardAPI
{
    public int money { get; set; }
    public List<ItemAPI> items { get; set; }
}

public static class Quest
{
    /// <summary>The sentence of a quest, from what it counts and how many times.</summary>
    public static string Describe(QuestAPI quest)
    {
        string goal = UIBuilder.Number(quest.goal);
        switch (quest.metric)
        {
            case "expeditions": return "Réalisez " + goal + " expéditions";
            case "floors": return "Terminez " + goal + " étages";
            case "clears": return "Menez " + goal + " expéditions jusqu'au dernier étage";
            case "monsters": return "Combattez " + goal + " monstres";
            case "items": return "Ramassez " + goal + " objets";
            case "equipment": return "Trouvez " + goal + " équipements";
            case "recruits": return "Invoquez " + goal + " personnages";
            case "sales": return "Vendez " + goal + " objets";
            case "digs": return "Creusez " + goal + " cases";
            // a quest added to the server after this version of the game
            default: return "Quête mystère (" + quest.metric + ") : " + goal;
        }
    }

    public static string Reward(QuestRewardAPI reward)
    {
        if (reward == null) return "";
        string text = UIBuilder.Number(reward.money) + " pièces";
        if (reward.items > 0) text += " + " + reward.items + (reward.items > 1 ? " objets" : " objet");
        return text;
    }

    /// <summary>"3 j 4 h", "5 h 12 min", "12 min" or "45 s".</summary>
    public static string Duration(float seconds)
    {
        int left = (int)System.Math.Max(0, System.Math.Ceiling(seconds));
        if (left >= 86400) return left / 86400 + " j " + left % 86400 / 3600 + " h";
        if (left >= 3600) return left / 3600 + " h " + left % 3600 / 60 + " min";
        if (left >= 60) return left / 60 + " min";
        return left + " s";
    }
}
