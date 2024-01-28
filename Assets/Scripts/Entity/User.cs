using Newtonsoft.Json;

[System.Serializable]
public class User
{
    public int id { get; set; }
    [JsonProperty("public_key")]
    public string publicKey { get; set; }

    [JsonProperty("private_key")]
    public string privateKey { get; set; }

    [JsonProperty("createdat")]
    public string created { get; set; }
    [JsonProperty("updatedat")]
    public string updated { get; set; }
    [JsonProperty("lastLogin")]
    public string lastLogin { get; set; }
    public int money { get; set; }

}