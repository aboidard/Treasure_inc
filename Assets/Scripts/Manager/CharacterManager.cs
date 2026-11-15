using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The characters of the user and their equipment, as known by the server: every change goes through the API,
/// which answers the characters afterwards. An equipped item leaves the inventory, and comes back when taken off.
/// </summary>
public class CharacterManager : MonoBehaviour
{
    private static CharacterManager _Instance;

    // local to the client, by character number - 1: the server only knows the numbers
    private static readonly string[] names = { "Kentucky Johanes", "Brok le quartier-maître", "Vax le caporal" };

    public List<GameCharacter> characters = new List<GameCharacter>();
    /// <summary>The portraits by character number - 1, given by the TeamPanel of the scene.</summary>
    private List<Sprite> portraits = new List<Sprite>();
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

    public void SetPortraits(List<Sprite> sprites)
    {
        portraits = sprites;
        foreach (GameCharacter character in characters)
        {
            character.graphics = Portrait(character.num);
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
        StartCoroutine(Request(NetworkManager.Instance.GetJson(NetworkManager.charactersEndPoint, Received), onDone, false));
    }

    /// <summary>Equips an item of the inventory in the slot of the character; the previous item goes back to the inventory.</summary>
    public void Equip(GameCharacter character, int slot, Item item, Action<bool> onDone = null)
    {
        string endPoint = String.Format(NetworkManager.equipItemEndPoint, "{0}", character.num);
        EquipItemRequest body = new EquipItemRequest { slot = slot, itemId = item.id };
        StartCoroutine(Request(NetworkManager.Instance.PostJson(endPoint, body, Received), onDone, true));
    }

    /// <summary>Empties the slot of the character: its item goes back to the inventory.</summary>
    public void Unequip(GameCharacter character, int slot, Action<bool> onDone = null)
    {
        string endPoint = String.Format(NetworkManager.unequipItemEndPoint, "{0}", character.num);
        UnequipItemRequest body = new UnequipItemRequest { slot = slot };
        StartCoroutine(Request(NetworkManager.Instance.PostJson(endPoint, body, Received), onDone, true));
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

    private long lastStatus;
    private string lastResponse;

    private void Received(long status, string response)
    {
        lastStatus = status;
        lastResponse = response;
    }

    private IEnumerator Request(IEnumerator call, Action<bool> onDone, bool showErrors)
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
            Apply(JsonConvert.DeserializeObject<List<CharacterAPI>>(lastResponse));
        }
        else if (showErrors)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", ErrorMessage(lastStatus, lastResponse));
        }
        else
        {
            Debug.LogWarning("Characters not loaded (" + lastStatus + ") : " + lastResponse);
        }
        onDone?.Invoke(ok);
    }

    private static string ErrorMessage(long status, string response)
    {
        switch (status)
        {
            case 409: return "Ce personnage est en expédition : son équipement ne peut pas être changé avant son retour.";
            case 404: return "Cet objet n'est plus dans votre inventaire.";
            case 400: return "Cet objet ne va pas à cet emplacement.";
            default: return "Impossible de changer l'équipement, veuillez réessayer ultérieurement !\n" + response;
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
                name = api.num >= 1 && api.num <= names.Length ? names[api.num - 1] : "Personnage " + api.num,
                graphics = Portrait(api.num),
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

    private Sprite Portrait(int num)
    {
        return num >= 1 && num <= portraits.Count ? portraits[num - 1] : null;
    }
}
