using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The characters of the user and their equipment, as known by the server: every change goes through the API,
/// which answers the characters afterwards. An equipped item leaves the inventory, and comes back when taken off.
/// The user starts with a few characters and recruits the others by drawing them in a portal (see Recruit).
/// </summary>
public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _Instance;

    public List<GameCharacter> characters = new List<GameCharacter>();
    /// <summary>The portals the characters are summoned from; empty until LoadPortals succeeded.</summary>
    public List<PortalAPI> portals = new List<PortalAPI>();
    public bool loaded = false;
    public bool isBusy = false;

    public static CharacterManager Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("CharacterManager").AddComponent<CharacterManager>();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }

    /// <summary>The portraits of the characters 1, 2, 3..., given by the TeamPanel of the scene (see CharacterArt).</summary>
    public void SetPortraits(List<Sprite> sprites)
    {
        CharacterArt.SetBases(sprites);
        foreach (GameCharacter character in characters)
        {
            character.graphics = CharacterArt.Portrait(character.num);
        }
    }

    public GameCharacter Get(int num)
    {
        return characters.FirstOrDefault(character => character.num == num);
    }

    public bool AnyAvailable()
    {
        // before the characters are known, let the server decide
        return !loaded || characters.Any(character => character.IsAvailable);
    }

    /// <summary>Reads the characters from the server, then calls onDone with true when they were received.</summary>
    public void Refresh(Action<bool> onDone = null)
    {
        StartCoroutine(Request(NetworkManager.Instance.GetJson(NetworkManager.charactersEndPoint, Received), onDone, null,
            response => Apply(JsonConvert.DeserializeObject<List<CharacterAPI>>(response))));
    }

    /// <summary>Equips an item of the inventory in the slot of the character; the previous item goes back to the inventory.</summary>
    public void Equip(GameCharacter character, int slot, Item item, Action<bool> onDone = null)
    {
        string endPoint = String.Format(NetworkManager.equipItemEndPoint, "{0}", character.num);
        EquipItemRequest body = new EquipItemRequest { slot = slot, itemId = item.id };
        StartCoroutine(Request(NetworkManager.Instance.PostJson(endPoint, body, Received), onDone, EquipmentError,
            response => Apply(JsonConvert.DeserializeObject<List<CharacterAPI>>(response))));
    }

    /// <summary>Empties the slot of the character: its item goes back to the inventory.</summary>
    public void Unequip(GameCharacter character, int slot, Action<bool> onDone = null)
    {
        string endPoint = String.Format(NetworkManager.unequipItemEndPoint, "{0}", character.num);
        UnequipItemRequest body = new UnequipItemRequest { slot = slot };
        StartCoroutine(Request(NetworkManager.Instance.PostJson(endPoint, body, Received), onDone, EquipmentError,
            response => Apply(JsonConvert.DeserializeObject<List<CharacterAPI>>(response))));
    }

    /// <summary>Reads the portals from the server, then calls onDone with true when they were received.</summary>
    public void LoadPortals(Action<bool> onDone = null)
    {
        StartCoroutine(Request(NetworkManager.Instance.GetJson(NetworkManager.portalsEndPoint, Received), onDone, null,
            response => portals = JsonConvert.DeserializeObject<List<PortalAPI>>(response) ?? new List<PortalAPI>()));
    }

    /// <summary>
    /// Pays for "count" draws in the portal. The server chooses the characters: onDone receives them in the order
    /// they were drawn, once the characters and the money of the user are up to date, or null when nothing was drawn
    /// (the message is already shown).
    /// </summary>
    public void Recruit(PortalAPI portal, int count, Action<RecruitResponse> onDone)
    {
        RecruitRequest body = new RecruitRequest { portal = portal.id, count = count };
        RecruitResponse drawn = null;
        StartCoroutine(Request(NetworkManager.Instance.PostJson(NetworkManager.recruitEndPoint, body, Received),
            ok => onDone?.Invoke(ok ? drawn : null), RecruitError,
            response =>
            {
                drawn = JsonConvert.DeserializeObject<RecruitResponse>(response);
                Apply(drawn.characters);
                Inventory.Instance.CurrentMoney = drawn.money;
            }));
    }

    /// <summary>The character leads this expedition until its chest is opened.</summary>
    public void MarkBusy(int num, string expeditionId)
    {
        GameCharacter character = Get(num);
        if (character != null) character.expeditionId = expeditionId;
    }

    /// <summary>The chest of the expedition is opened: its character is available again.</summary>
    public void MarkFree(string expeditionId)
    {
        foreach (GameCharacter character in characters)
        {
            if (character.expeditionId == expeditionId) character.expeditionId = null;
        }
    }

    /// <summary>The character earned experience: its level and its stats are those the server answered.</summary>
    public void ApplyXp(XpGainAPI gain)
    {
        GameCharacter character = gain != null ? Get(gain.character) : null;
        if (character == null) return;
        character.level = Mathf.Max(1, gain.level);
        character.xp = gain.xp;
        character.xpNext = gain.xpNext;
        if (gain.stats != null) character.stats = gain.stats;
    }

    private long lastStatus;
    private string lastResponse;

    private void Received(long status, string response)
    {
        lastStatus = status;
        lastResponse = response;
    }

    /// <param name="errorMessage">the message shown for a status and a response when the request fails; null: no message</param>
    /// <param name="onResponse">reads the response of a successful request</param>
    private IEnumerator Request(IEnumerator call, Action<bool> onDone, Func<long, string, string> errorMessage,
        Action<string> onResponse)
    {
        // one request at a time: the answer of a request replaces every character
        while (isBusy) yield return null;
        isBusy = true;
        Loader.instance.SetLoading(true);
        yield return call;
        Loader.instance.SetLoading(false);
        isBusy = false;

        bool ok = lastStatus == 200;
        if (ok)
        {
            onResponse(lastResponse);
        }
        else if (errorMessage != null)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", errorMessage(lastStatus, lastResponse));
        }
        else
        {
            Debug.LogWarning("Characters request failed (" + lastStatus + ") : " + lastResponse);
        }
        onDone?.Invoke(ok);
    }

    private static string EquipmentError(long status, string response)
    {
        switch (status)
        {
            case 409: return "Ce personnage est en expédition : son équipement ne peut pas être changé avant son retour.";
            case 404: return "Cet objet n'est plus dans votre inventaire.";
            case 400: return "Cet objet ne va pas à cet emplacement.";
            default: return "Impossible de changer l'équipement, veuillez réessayer ultérieurement !\n" + response;
        }
    }

    private static string RecruitError(long status, string response)
    {
        switch (status)
        {
            case 402: return "Vous n'avez pas assez d'argent pour cette invocation.";
            case 404: return "Ce portail n'existe plus.";
            default: return "Le portail ne répond pas, veuillez réessayer ultérieurement !\n" + response;
        }
    }

    /// <summary>
    /// Replaces the characters by those of the server and keeps the inventory in line: the items that are no
    /// longer worn come back to it, those that are now worn leave it.
    /// </summary>
    private void Apply(List<CharacterAPI> fromServer)
    {
        Dictionary<int, Item> before = characters.SelectMany(character => character.equipment.Values)
            .ToDictionary(item => item.id);

        List<GameCharacter> updated = new List<GameCharacter>();
        foreach (CharacterAPI api in fromServer.OrderBy(c => c.num))
        {
            GameCharacter character = new GameCharacter
            {
                num = api.num,
                name = string.IsNullOrEmpty(api.name) ? "Personnage " + api.num : api.name,
                rank = string.IsNullOrEmpty(api.rank) ? "D" : api.rank,
                copies = Mathf.Max(1, api.copies),
                level = Mathf.Max(1, api.level),
                xp = api.xp,
                xpNext = api.xpNext,
                stats = api.stats,
                graphics = CharacterArt.Portrait(api.num),
                expeditionId = api.expeditionId,
            };
            foreach (var worn in api.equipment ?? new Dictionary<string, ItemAPI>())
            {
                // the same Item object is kept while it stays equipped
                character.equipment[int.Parse(worn.Key)] = before.TryGetValue(worn.Value.id, out Item known)
                    ? known : Item.CreateItemAPI(worn.Value);
            }
            updated.Add(character);
        }
        characters = updated;
        loaded = true;

        Dictionary<int, Item> after = characters.SelectMany(character => character.equipment.Values)
            .ToDictionary(item => item.id);
        List<Item> inventory = Inventory.Instance.Items;
        inventory.RemoveAll(item => after.ContainsKey(item.id));
        foreach (Item item in before.Values)
        {
            if (!after.ContainsKey(item.id) && !inventory.Any(owned => owned.id == item.id))
            {
                Inventory.Instance.AddItem(item);
            }
        }
    }
}
