using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager instance;
    public List<Expedition> expeditionList;
    public GameObject expeditionPanelPrefab;
    public GameObject expeditionsPanelGrid;
    public GameObject LootPanelGrid;
    public GameObject LootButtonPrefab;
    public bool isSending;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    void Start()
    {
        expeditionList = new List<Expedition>();
    }

    private void FixedUpdate()
    {
        UpdateExpeditions();
    }

    private void UpdateExpeditions()
    {
        List<Expedition> expeditionToRemove = null;
        //traitement des expeditions
        foreach (var expedition in expeditionList)
        {
            //expedition.Update();
            if (expedition.floorOver)
            {
                expedition.currentFloor++;
                //calcul si l'expedition est finie : dernier étage atteint, ou personnage mort en chemin
                if (expedition.failed || expedition.currentFloor > expedition.nbTtotalFloor)
                {
                    expedition.over = true;
                    ExpeditionReturn(expedition);
                    if (expeditionToRemove == null)
                    {
                        expeditionToRemove = new List<Expedition>();
                    }
                    expeditionToRemove.Add(expedition);
                    Destroy(expedition.panelRun);
                }
                // si l'expedition n'est pas terminée on continu dans un autre étage
                else
                {
                    Destroy(expedition.panelRun);
                    expedition.panelRun = Instantiate(expeditionPanelPrefab, expedition.positionInGrid.transform);
                    expedition.panelRun.GetComponent<ExpeditionRunPanel>().expedition = expedition;
                    expedition.floorOver = false;
                    expedition.panelRun.GetComponent<ExpeditionRunPanel>().updateUI();
                }
            }
        }

        CleanExpeditions(expeditionToRemove);
    }

    private void CleanExpeditions(List<Expedition> listToRemove)
    {
        if (listToRemove != null)
        {
            foreach (Expedition expedition in listToRemove)
            {
                expeditionList.Remove(expedition);
            }
        }
    }

    /// <summary>
    /// Asks the API to send an expedition led by a character; it starts here, and onSuccess is called, once the API
    /// accepts it.
    /// </summary>
    public void SendExpedition(Location location, int floor, int? caseId, GameCharacter character, Action onSuccess = null)
    {
        if (expeditionList.Count >= 5) throw new TooMuchExpeditionsException();

        StartCoroutine(PostExpedition(location, floor, caseId, character, onSuccess));
    }

    /// <summary>
    /// After a restart the expeditions under way are lost, but the server still keeps their character busy until
    /// their chest is opened: such an expedition comes back at once, so its chest can be opened.
    /// </summary>
    public void RecoverLostExpeditions()
    {
        foreach (GameCharacter character in CharacterManager.Instance.characters)
        {
            if (character.IsAvailable || expeditionList.Any(e => e.expeditionId == character.expeditionId)) continue;
            if (LootPanelGrid.GetComponentsInChildren<LootButton>().Any(b => b.ex != null && b.ex.expeditionId == character.expeditionId)) continue;

            Expedition lost = new Expedition();
            lost.expeditionId = character.expeditionId;
            lost.expeditionName = "Expédition de " + character.name;
            lost.character = character;
            lost.over = true;
            ExpeditionReturn(lost);
        }
    }

    private IEnumerator PostExpedition(Location location, int floor, int? caseId, GameCharacter character, Action onSuccess)
    {
        isSending = true;
        Loader.instance.SetLoading(true);
        SendExpeditionRequest body = new SendExpeditionRequest { location = location.name, difficulty = floor, caseId = caseId, character = character.num };
        long status = 0;
        string response = null;
        yield return NetworkManager.Instance.PostJson(NetworkManager.sendExpeditionEndPoint, body, (code, text) =>
        {
            status = code;
            response = text;
        });
        Loader.instance.SetLoading(false);
        isSending = false;

        if (status == 402)
        {
            MessagePanel.instance.DisplayMessage("Pas assez d'argent !", "Vous n'avez pas assez d'argent pour envoyer cette expédition.");
            yield break;
        }
        if (status == 409)
        {
            MessagePanel.instance.DisplayMessage("En expédition", character.name + " est déjà en expédition : ouvrez son coffre pour le revoir.");
            CharacterManager.Instance.Refresh(ok => { if (ok) RecoverLostExpeditions(); });
            yield break;
        }
        if (status != 200)
        {
            MessagePanel.instance.DisplayMessage("Erreur !", "Impossible d'envoyer l'expédition, veuillez réessayer ultérieurement !\n" + response);
            yield break;
        }

        SendExpeditionResponse sent = JsonConvert.DeserializeObject<SendExpeditionResponse>(response);
        Inventory.Instance.CurrentMoney = sent.money;
        Expedition expedition = new Expedition();
        expedition.expeditionId = sent.expeditionId;
        expedition.itemCount = sent.itemCount;
        expedition.expeditionName = StringGenerator.ExpeditionNameGenerator();
        expedition.location = location;
        expedition.nbTtotalFloor = floor;
        expedition.character = character;
        // the obstacles and the fights of the way, already decided by the server
        expedition.run = sent.run;
        if (sent.run != null)
        {
            expedition.maxHp = sent.run.maxHp;
            expedition.hp = sent.run.maxHp;
        }
        CharacterManager.Instance.MarkBusy(character.num, sent.expeditionId);
        expedition.positionInGrid = ExpeditionGridPanel.instance.GetFreeSpot(ExpeditionGridPanel.SIZE_S);
        expedition.panelRun = Instantiate(expeditionPanelPrefab, expedition.positionInGrid.transform);
        expedition.panelRun.GetComponent<ExpeditionRunPanel>().SetExpedition(expedition);
        expedition.Init();
        expeditionList.Add(expedition);
        SoundManager.Play(Sfx.ExpeditionSend);

        Debug.Log("Expedition send " + expedition);
        onSuccess?.Invoke();
    }

    public void ExpeditionReturn(Expedition ex)
    {
        Debug.Log("Expedition returned : " + ex);
        // the defeat was already heard when the character fell
        if (!ex.failed) SoundManager.Play(Sfx.ExpeditionReturn);
        GameObject lootButton = Instantiate(LootButtonPrefab, LootPanelGrid.transform);
        lootButton.GetComponent<LootButton>().ex = ex;
    }

    /// <summary>The cost to display before sending: what is really paid is decided by the engine.</summary>
    public int ComputeCost(int time, Location location)
    {
        int cost = time * 100 * location.level;

        return cost;
    }
    public Item GenerateItem()
    {
        int randomInt = UnityEngine.Random.Range(1, ItemsDatabase.instance.allItems.Length + 1);
        return ItemsDatabase.instance.allItems.Single(x => x.id == randomInt);
    }
}
