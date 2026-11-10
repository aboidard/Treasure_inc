using Newtonsoft.Json;

[System.Serializable]
public class User
{
    public int id { get; set; }
    [JsonProperty("public_key")]
    public string publicKey { get; set; }
<<<<<<< HEAD
    [JsonProperty("private_key")]
    public string privateKey { get; set; }
=======

    [JsonProperty("private_key")]
    public string privateKey { get; set; }

>>>>>>> ea0301e2b0d59b8838a0fb7178f39ce6e39d4cef
    [JsonProperty("createdat")]
    public string created { get; set; }
    [JsonProperty("updatedat")]
    public string updated { get; set; }
    [JsonProperty("lastLogin")]
    public string lastLogin { get; set; }
    public int money { get; set; }

}