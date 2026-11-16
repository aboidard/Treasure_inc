using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// The daily and weekly quests of the user, as known by the server. The server counts what the user does: the
/// client only reads the quests again after an action that may count (Refresh), and asks for the reward of a
/// completed quest (Claim). Changed is raised each time the quests were received.
/// </summary>
public class QuestManager : MonoBehaviour
{
    private static QuestManager _Instance;

    // after a failed reading at the end of a period, the next one waits
    private const float retryDelay = 15f;

    /// <summary>Null until the quests were received once.</summary>
    public QuestsAPI quests;
    public bool isBusy = false;
    public event Action Changed;

    private float receivedAt;
    private bool reading;
    private bool readAgain;
    private float nextAutoRefresh;

    public static QuestManager Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("QuestManager").AddComponent<QuestManager>();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }

    /// <summary>How many rewards wait to be taken.</summary>
    public int Claimable => quests != null ? quests.claimable : 0;

    /// <summary>The seconds left before the board is replaced by a new one.</summary>
    public float SecondsLeft(QuestBoardAPI board)
    {
        return Mathf.Max(0f, board.resetIn - (Time.realtimeSinceStartup - receivedAt));
    }

    /// <summary>
    /// Reads the quests from the server, without the loading screen: it is called after the actions that may count
    /// for a quest. A reading asked while another one is under way is made after it, as the first one may have been
    /// answered before the action was counted.
    /// </summary>
    public void Refresh()
    {
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
            yield return NetworkManager.Instance.GetJson(NetworkManager.questsEndPoint, (code, text) =>
            {
                status = code;
                response = text;
            });
            if (status == 200)
            {
                Apply(JsonConvert.DeserializeObject<QuestsAPI>(response));
            }
            else
            {
                Debug.LogWarning("Quests request failed (" + status + ") : " + response);
            }
        } while (readAgain);
        reading = false;
    }

    /// <summary>
    /// Takes the reward of a completed quest: the money and the items are added to those of the user. onDone
    /// receives what the quest gave and its items, now in the inventory, or nulls when nothing was given (the
    /// message is already shown).
    /// </summary>
    public void Claim(QuestAPI quest, Action<QuestClaimResponse, List<Item>> onDone)
    {
        if (isBusy) return;
        StartCoroutine(Claiming(quest, onDone));
    }

    private IEnumerator Claiming(QuestAPI quest, Action<QuestClaimResponse, List<Item>> onDone)
    {
        isBusy = true;
        Loader.instance.SetLoading(true);
        long status = 0;
        string response = null;
        string endPoint = String.Format(NetworkManager.claimQuestEndPoint, "{0}", quest.id);
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

        QuestClaimResponse claimed = JsonConvert.DeserializeObject<QuestClaimResponse>(response);
        Inventory.Instance.CurrentMoney = claimed.money;
        List<Item> given = new List<Item>();
        foreach (ItemAPI it in claimed.reward?.items ?? new List<ItemAPI>())
        {
            Item item = Item.CreateItemAPI(it);
            given.Add(item);
            Inventory.Instance.AddItem(item);
        }
        if (claimed.quests != null) Apply(claimed.quests);
        onDone?.Invoke(claimed, given);
    }

    private static string ClaimError(long status, string response)
    {
        switch (status)
        {
            case 404: return "Cette quête n'est plus disponible : de nouvelles quêtes l'ont remplacée.";
            case 409: return "Cette quête n'est pas terminée, ou sa récompense a déjà été récupérée.";
            default: return "Impossible de récupérer la récompense, veuillez réessayer ultérieurement !\n" + response;
        }
    }

    private void Apply(QuestsAPI fromServer)
    {
        if (fromServer == null) return;
        if (fromServer.daily == null) fromServer.daily = new QuestBoardAPI();
        if (fromServer.weekly == null) fromServer.weekly = new QuestBoardAPI();
        if (fromServer.daily.quests == null) fromServer.daily.quests = new List<QuestAPI>();
        if (fromServer.weekly.quests == null) fromServer.weekly.quests = new List<QuestAPI>();
        quests = fromServer;
        receivedAt = Time.realtimeSinceStartup;
        Changed?.Invoke();
    }

    private void Update()
    {
        // a period is over: the server has new quests
        if (quests == null || !NetworkManager.logedIn || reading || Time.realtimeSinceStartup < nextAutoRefresh) return;
        if (SecondsLeft(quests.daily) <= 0f || SecondsLeft(quests.weekly) <= 0f)
        {
            nextAutoRefresh = Time.realtimeSinceStartup + retryDelay;
            Refresh();
        }
    }
}
