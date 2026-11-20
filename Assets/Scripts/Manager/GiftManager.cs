using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The gift box of the user, as known by the server. The gifts arrive at any time (an administrator sends them):
/// the box is read when the game starts, when it is opened, and every few minutes (Refresh). Claim takes a gift.
/// Changed is raised each time the box was received.
/// </summary>
public class GiftManager : MonoBehaviour
{
    private static GiftManager _Instance;

    // how often the box is read again while the game is open
    private const float refreshPeriod = 300f;

    /// <summary>Null until the box was received once.</summary>
    public GiftsAPI gifts;
    public bool isBusy = false;
    public event Action Changed;

    private bool reading;
    private bool readAgain;
    private float nextRefresh;

    public static GiftManager Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("GiftManager").AddComponent<GiftManager>();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }

    /// <summary>How many gifts wait to be taken.</summary>
    public int Count => gifts != null ? gifts.count : 0;

    /// <summary>Reads the box from the server, without the loading screen.</summary>
    public void Refresh()
    {
        nextRefresh = Time.realtimeSinceStartup + refreshPeriod;
        if (reading)
        {
            readAgain = true;
            return;
        }
        StartCoroutine(Read());
    }

    private IEnumerator Read()
    {
        reading = true;
        do
        {
            readAgain = false;
            long status = 0;
            string response = null;
            yield return NetworkManager.Instance.GetJson(NetworkManager.giftsEndPoint, (code, text) =>
            {
                status = code;
                response = text;
            });
            if (status == 200)
            {
                Apply(JsonConvert.DeserializeObject<GiftsAPI>(response));
            }
            else
            {
                Debug.LogWarning("Gifts request failed (" + status + ") : " + response);
            }
        } while (readAgain);
        reading = false;
    }

    /// <summary>
    /// Takes a gift: its money, its gems and its items are added to those of the user. onDone receives what the
    /// gift gave and its items, now in the inventory, or nulls when nothing was given (the message is already shown).
    /// </summary>
    public void Claim(GiftAPI gift, Action<GiftClaimResponse, List<Item>> onDone)
    {
        if (isBusy) return;
        StartCoroutine(Claiming(gift, onDone));
    }

    private IEnumerator Claiming(GiftAPI gift, Action<GiftClaimResponse, List<Item>> onDone)
    {
        isBusy = true;
        Loader.instance.SetLoading(true);
        long status = 0;
        string response = null;
        string endPoint = String.Format(NetworkManager.claimGiftEndPoint, "{0}", gift.id);
        yield return NetworkManager.Instance.PostJson(endPoint, new Dictionary<string, object>(), (code, text) =>
        {
            status = code;
            response = text;
        });
        Loader.instance.SetLoading(false);
        isBusy = false;

        if (status != 200)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", ClaimError(status, response));
            // what the client shows is not what the server knows
            Refresh();
            onDone?.Invoke(null, null);
            yield break;
        }

        GiftClaimResponse claimed = JsonConvert.DeserializeObject<GiftClaimResponse>(response);
        Inventory.Instance.CurrentMoney = claimed.money;
        Inventory.Instance.CurrentGems = claimed.gems;
        List<Item> given = new List<Item>();
        foreach (ItemAPI it in claimed.reward?.items ?? new List<ItemAPI>())
        {
            Item item = Item.CreateItemAPI(it);
            given.Add(item);
            Inventory.Instance.AddItem(item);
        }
        if (claimed.gifts != null) Apply(claimed.gifts);
        onDone?.Invoke(claimed, given);
    }

    private static string ClaimError(long status, string response)
    {
        switch (status)
        {
            case 404: return "Ce cadeau n'est plus dans votre boîte.";
            case 409: return "Ce cadeau a déjà été récupéré.";
            case 410: return "Trop tard : ce cadeau a expiré !";
            default: return "Impossible de récupérer le cadeau, veuillez réessayer ultérieurement !\n" + response;
        }
    }

    private void Apply(GiftsAPI fromServer)
    {
        if (fromServer == null) return;
        if (fromServer.gifts == null) fromServer.gifts = new List<GiftAPI>();
        gifts = fromServer;
        Changed?.Invoke();
    }

    private void Update()
    {
        if (NetworkManager.logedIn && !reading && Time.realtimeSinceStartup >= nextRefresh) Refresh();
    }
}
