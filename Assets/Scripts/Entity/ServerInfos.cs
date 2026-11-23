using System;
using System.Globalization;
using Newtonsoft.Json;

/// <summary>
/// The answer of GET /healthcheck of the API: the server version, the maintenance and the oldest version of the
/// client allowed to log in.
/// </summary>
[System.Serializable]
public class ServerInfos
{
    public string version { get; set; }
    public MaintenanceInfos maintenance { get; set; }
    // "0": any version
    public string minClientVersion { get; set; }

    /// <summary>Keeps the ISO-8601 times as they are: by default Json.NET rewrites them in the format of the phone.</summary>
    public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };

    public static ServerInfos Parse(string json) => JsonConvert.DeserializeObject<ServerInfos>(json, JsonSettings);

    /// <summary>A maintenance is under way: the game cannot be played.</summary>
    public bool InMaintenance => maintenance != null && maintenance.IsOn;

    /// <summary>This client is older than the oldest version the server accepts.</summary>
    public bool UpdateRequired => !string.IsNullOrEmpty(minClientVersion)
        && CompareVersions(UnityEngine.Application.version, minClientVersion) < 0;

    /// <summary>
    /// Compares two versions made of numbers separated by dots ("0.22" &lt; "0.23" &lt; "1.0"); a missing number counts as 0
    /// and a part that is not a number as 0.
    /// </summary>
    public static int CompareVersions(string a, string b)
    {
        string[] pa = (a ?? "0").Split('.');
        string[] pb = (b ?? "0").Split('.');
        for (int i = 0; i < Math.Max(pa.Length, pb.Length); i++)
        {
            long na = i < pa.Length && long.TryParse(pa[i], NumberStyles.None, CultureInfo.InvariantCulture, out long x) ? x : 0;
            long nb = i < pb.Length && long.TryParse(pb[i], NumberStyles.None, CultureInfo.InvariantCulture, out long y) ? y : 0;
            if (na != nb) return na < nb ? -1 : 1;
        }
        return 0;
    }
}

/// <summary>
/// The maintenance as the API tells it, in the healthcheck and in the 503 answers of the game routes: status "on" while
/// it is under way; reason, start and end (ISO-8601) as soon as one is planned.
/// </summary>
[System.Serializable]
public class MaintenanceInfos
{
    public string status { get; set; }
    public string reason { get; set; }
    public string start { get; set; }
    public string end { get; set; }

    public bool IsOn => status == "on";

    /// <summary>The planned end in the local time of the phone, null when none is planned.</summary>
    public DateTime? LocalEnd
    {
        get
        {
            if (string.IsNullOrEmpty(end)) return null;
            return DateTime.TryParse(end, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime t)
                ? t.ToLocalTime() : (DateTime?)null;
        }
    }

    /// <summary>What the player reads: the reason and, when it is planned, the end.</summary>
    public string Text
    {
        get
        {
            string text = string.IsNullOrEmpty(reason) ? "Le jeu est en maintenance." : reason;
            DateTime? localEnd = LocalEnd;
            if (localEnd.HasValue)
            {
                text += "\n\nFin prévue le " + localEnd.Value.ToString("dd/MM/yyyy 'à' HH:mm", CultureInfo.GetCultureInfo("fr-FR"));
            }
            return text;
        }
    }
}

/// <summary>Body of a 503 of the API during a maintenance: {error: "maintenance", message, maintenance}.</summary>
[System.Serializable]
public class MaintenanceError
{
    public string error { get; set; }
    public string message { get; set; }
    public MaintenanceInfos maintenance { get; set; }

    /// <summary>The maintenance told by an answer of the API, or null when it is not a maintenance.</summary>
    public static MaintenanceInfos Read(long code, string text)
    {
        if (code != 503 || string.IsNullOrEmpty(text)) return null;
        try
        {
            MaintenanceError e = JsonConvert.DeserializeObject<MaintenanceError>(text, ServerInfos.JsonSettings);
            if (e == null || e.error != "maintenance") return null;
            MaintenanceInfos m = e.maintenance ?? new MaintenanceInfos { status = "on" };
            if (string.IsNullOrEmpty(m.reason)) m.reason = e.message;
            return m;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
