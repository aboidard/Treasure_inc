using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A floor of an expedition: the character walks to the chest, faster when the panel is touched. What stands in
/// its way was decided by the server when the expedition was sent (Expedition.run): this panel only shows it.
///   - a barricade stops the character for a while, the time it takes to destroy it;
///   - a monster is fought: the fight is played as the server computed it, event after event;
///   - the chest may be kept by a guardian, a monster that leaves the chest when it dies;
///   - when the life of the character falls to zero, the expedition comes back at once.
/// The obstacles, the life bars and the texts of the fights are built by code: the prefab is not modified.
/// </summary>
public class ExpeditionRunPanel : MonoBehaviour
{
    public Expedition expedition;
    public Rigidbody2D rb;
    public bool endMiniGame = false;
    public GameObject crewViewport;
    public GameObject crewMemberPrefab;
    private GameObject crewMember;
    public GameObject expeditionRunPanel;
    // the finger of the prefab that told to touch the screen: it is not shown anymore
    public Image touchImage;
    public GameObject targetViewport;
    private GameObject Background;
    private Event target;
    public Event[] targetAvailableList;
    public Text title;
    public Text lootNumberText;
    public float walkSpeed = 7000f;
    public float initialMagnitude = 140f;
    public int size = ExpeditionGridPanel.SIZE_S;

    // how far from the center of the character an obstacle stands when it stops it, in the units of the panel
    private const float obstacleGap = 105f;
    private const float obstacleHeight = 100f;
    private const float guardianHeight = 130f;
    // the center of the character is about this far from the center of the chest when it reaches it
    private const float chestGap = 66f;
    private const float fightSoundVolume = 0.5f;
    private static readonly Color damageColor = new Color(1f, 0.9f, 0.6f);
    private static readonly Color criticalColor = new Color(1f, 0.75f, 0.1f);
    private static readonly Color hurtColor = new Color(1f, 0.35f, 0.3f);
    private static readonly Color healColor = new Color(0.45f, 1f, 0.45f);
    private static readonly Color skillColor = new Color(0.6f, 0.85f, 1f);
    private static readonly Color poisonColor = new Color(0.7f, 1f, 0.3f);

    /// <summary>An obstacle of the floor that is not reached yet: where the character stops, and what is shown there.</summary>
    private class Obstacle
    {
        public EncounterAPI encounter;
        public float stopX;
        public EncounterView view;
    }

    private readonly List<Obstacle> obstacles = new List<Obstacle>();
    private EncounterAPI guardian;
    private EncounterView guardianView;
    private CanvasGroup chestGroup;
    private RectTransform crewImage;
    private Slider healthBar;
    private Text healthText;
    private Image healthFill;
    // the green of the prefab: the color of the bar of a character in good health
    private Color healthyColor;
    private static readonly Color woundedColor = new Color(1f, 0.75f, 0.1f);
    private static readonly Color dyingColor = new Color(0.95f, 0.2f, 0.15f);
    private RigidbodyConstraints2D walkingConstraints;
    // true while the character is stopped by a barricade or a fight
    private bool stopped;

    private void Start()
    {
        if (touchImage != null) touchImage.gameObject.SetActive(false);
        crewMember =Instantiate(crewMemberPrefab, crewViewport.transform);
        // the character leading the expedition is the one walking
        Transform crewImageTransform = crewMember.transform.Find("CrewImage");
        crewImage = crewImageTransform as RectTransform;
        if (crewImageTransform != null && expedition != null && expedition.character != null && expedition.character.graphics != null)
        {
            Image image = crewImageTransform.GetComponent<Image>();
            image.sprite = expedition.character.graphics;
            image.preserveAspect = true;
        }
        Background = Instantiate(BackgroundDatabase.instance.PickBackground(Biome.MINE), this.transform);
        Background.transform.SetSiblingIndex(0);
        crewMember.transform.SetSiblingIndex(0);
        lootNumberText.text = "0";

        target = EventManager.instance.GenerateRandomEvent(targetAvailableList, targetViewport);
        target.transform.position = targetViewport.transform.position;
        initialMagnitude = 140f;
        walkingConstraints = rb.constraints;
        InitHealth();
        InitEncounters();
        rb.AddForce(new Vector2(walkSpeed, 0f));
        updateUI();
    }

    private void FixedUpdate()
    {
        if (!endMiniGame)
        {
            UpdateCrewPosition();
        }
        if (!endMiniGame && !stopped)
        {
            if (obstacles.Count > 0 && CrewX() >= obstacles[0].stopX)
            {
                Obstacle obstacle = obstacles[0];
                obstacles.RemoveAt(0);
                StartCoroutine(Meet(obstacle.encounter, obstacle.view, false));
            }
            else if (targetViewport.GetComponent<EventTrigger>().isInRange)
            {
                if (guardian != null)
                {
                    EncounterAPI met = guardian;
                    guardian = null;
                    StartCoroutine(Meet(met, guardianView, true));
                }
                else
                {
                    ReachChest();
                }
            }
        }
    }

    /// <summary>The end of the floor: the chest opens, then the next floor starts.</summary>
    private void ReachChest()
    {
        Stop();
        target.Reach();
        SoundManager.Play(Sfx.EventReach, 1f, 0.6f);
        expedition.floorsCompleted++;
        endMiniGame = true;
        ExpeditionFloorAPI floor = expedition.CurrentFloor;
        if (floor != null)
        {
            // what the character recovers at the end of a floor
            if (floor.healed > 0) Float(CrewPosition() + new Vector2(0f, 60f), "+" + floor.healed + " PV", healColor, 34);
            SetHealth(floor.hpEnd);
        }
        updateUI();
        StartCoroutine(WaitForEnding());
    }

    private IEnumerator WaitForEnding()
    {
        yield return new WaitForSeconds(1.5f);

        expedition.floorOver = true;
    }


    public void UpdateCrewPosition()
    {
        crewMember.transform.position = crewViewport.transform.position;
        //if the crew velocity is > walkSpeed, then the velocity is gradualy reduced to walkSpeed

        if (!stopped && rb.velocity.magnitude > initialMagnitude)
        {
            rb.AddForce(new Vector2(-walkSpeed / 60, 0f));
        }
    }

    public void updateUI()
    {
        title.text = expedition.expeditionName + " - étage " + expedition.currentFloor + "/" + expedition.nbTtotalFloor;
        lootNumberText.text = expedition.LootFound().ToString();
    }

    public void Touch()
    {
        // touching does not hurry a barricade or a fight
        if (endMiniGame || stopped) return;
        SoundManager.Play(Sfx.Step, 1f, 0.7f);
        rb.AddForce(new Vector2(walkSpeed, 0f));
        UpdateCrewPosition();
    }

    public void SetExpedition(Expedition expedition)
    {
        this.expedition = expedition;
        updateUI();
    }

    // ---- obstacles and fights

    /// <summary>The life bar of the walking character: hidden when the server sent no fight.</summary>
    private void InitHealth()
    {
        healthBar = crewMember.GetComponentInChildren<Slider>(true);
        if (healthBar == null) return;
        healthText = healthBar.GetComponentInChildren<Text>(true);
        if (healthText == null) healthText = crewMember.GetComponentsInChildren<Text>(true).FirstOrDefault();
        if (expedition == null || expedition.run == null || expedition.maxHp <= 0)
        {
            healthBar.gameObject.SetActive(false);
            if (healthText != null) healthText.gameObject.SetActive(false);
            return;
        }
        if (healthBar.fillRect != null) healthFill = healthBar.fillRect.GetComponent<Image>();
        if (healthFill != null) healthyColor = healthFill.color;
        // the bar only shows the life: it must not take the touches that hurry the character
        healthBar.interactable = false;
        healthBar.transition = Selectable.Transition.None;
        healthBar.wholeNumbers = false;
        healthBar.minValue = 0;
        healthBar.maxValue = expedition.maxHp;
        SetHealth(expedition.hp);
    }

    private void SetHealth(int hp)
    {
        expedition.hp = Mathf.Clamp(hp, 0, Mathf.Max(1, expedition.maxHp));
        if (healthBar != null) healthBar.value = expedition.hp;
        if (healthFill != null)
        {
            // green above half of the life, then turning orange, then red when a quarter is left
            float left = expedition.hp / (float)Mathf.Max(1, expedition.maxHp);
            healthFill.color = left >= 0.5f ? Color.Lerp(woundedColor, healthyColor, (left - 0.5f) * 4f)
                : Color.Lerp(dyingColor, woundedColor, (left - 0.25f) * 4f);
        }
        if (healthText != null) healthText.text = expedition.hp + "/" + expedition.maxHp;
    }

    /// <summary>Places what the server planned on this floor: the obstacles on the way, the guardian on the chest.</summary>
    private void InitEncounters()
    {
        ExpeditionFloorAPI floor = expedition != null ? expedition.CurrentFloor : null;
        if (floor == null || floor.encounters == null) return;

        float startX = CrewX();
        float endX = PanelPosition(targetViewport.transform).x - chestGap;
        float y = CrewPosition().y - 20f;
        foreach (EncounterAPI encounter in floor.encounters.OrderBy(e => e.position))
        {
            if (encounter.IsGuardian || encounter.position >= 100)
            {
                // one guardian a floor: it hides the chest it keeps
                if (guardian != null) continue;
                guardian = encounter;
                chestGroup = target.gameObject.GetComponent<CanvasGroup>();
                if (chestGroup == null) chestGroup = target.gameObject.AddComponent<CanvasGroup>();
                chestGroup.alpha = 0f;
                guardianView = EncounterView.Create(targetViewport.transform, new Vector2(0f, -10f), guardianHeight, encounter, title);
                continue;
            }
            float stopX = Mathf.Lerp(startX, endX, Mathf.Clamp(encounter.position, 5, 95) / 100f);
            EncounterView view = EncounterView.Create(transform, new Vector2(stopX + obstacleGap, y), obstacleHeight, encounter, title);
            // above the background, below the character and the title
            view.transform.SetSiblingIndex(1);
            obstacles.Add(new Obstacle { encounter = encounter, stopX = stopX, view = view });
        }
    }

    /// <summary>The character is stopped by a barricade or a monster, then walks again, unless it died.</summary>
    private IEnumerator Meet(EncounterAPI encounter, EncounterView view, bool keepsChest)
    {
        Stop();
        bool alive = true;
        if (encounter.IsBarricade || encounter.fight == null)
        {
            yield return BreakBarricade(encounter, view);
        }
        else
        {
            yield return Fight(encounter, view);
            alive = encounter.fight.win;
        }

        if (!alive)
        {
            yield return Die();
            yield break;
        }
        if (keepsChest)
        {
            // the guardian is dead: its chest appears
            for (float time = 0f; time < 0.3f; time += Time.deltaTime)
            {
                if (chestGroup != null) chestGroup.alpha = time / 0.3f;
                yield return null;
            }
            if (chestGroup != null) chestGroup.alpha = 1f;
            stopped = false;
            ReachChest();
            yield break;
        }
        Walk();
    }

    /// <summary>The character hits the barricade for as long as the server said, then the barricade breaks.</summary>
    private IEnumerator BreakBarricade(EncounterAPI encounter, EncounterView view)
    {
        float duration = Mathf.Max(0.5f, encounter.duration / 1000f);
        // the blows follow the speed of the character, when it is known
        float blow = expedition.character != null && expedition.character.combat != null
            ? Mathf.Clamp(expedition.character.combat.interval / 1000f, 0.3f, 2f) : 0.8f;
        float nextBlow = blow / 2f;
        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            if (view != null) view.SetHp(encounter.hp * (1f - time / duration));
            if (time >= nextBlow)
            {
                nextBlow += blow;
                Lunge();
                if (view != null) view.Shake();
                SoundManager.Play(Sfx.Hit, 0.8f, fightSoundVolume);
            }
            yield return null;
        }
        SoundManager.Play(Sfx.BarricadeBreak, 1f, 0.7f);
        if (view != null) yield return view.Vanish();
    }

    /// <summary>Plays a fight as the server computed it: each event is shown at its time.</summary>
    private IEnumerator Fight(EncounterAPI encounter, EncounterView view)
    {
        FightAPI fight = encounter.fight;
        List<FightEventAPI> events = fight.events ?? new List<FightEventAPI>();
        SetHealth(fight.hpStart);
        int next = 0;
        float duration = fight.duration / 1000f;
        for (float time = 0f; time < duration || next < events.Count; time += Time.deltaTime)
        {
            while (next < events.Count && events[next].t <= time * 1000f)
            {
                Show(events[next], view);
                next++;
            }
            yield return null;
        }

        if (fight.win)
        {
            SetHealth(fight.hpEnd);
            SoundManager.Play(Sfx.MonsterDown, 1f, 0.7f);
            if (view != null) yield return view.Vanish();
        }
        else
        {
            SetHealth(0);
        }
    }

    private void Show(FightEventAPI e, EncounterView view)
    {
        Vector2 crew = CrewPosition() + new Vector2(0f, 50f);
        Vector2 monster = view != null ? PanelPosition(view.transform) + new Vector2(0f, 40f) : crew + new Vector2(obstacleGap, 0f);
        // several events of the same moment do not hide each other
        Vector2 spread = new Vector2(Random.Range(-25f, 25f), Random.Range(-12f, 12f));
        bool named = !string.IsNullOrEmpty(e.skill) && e.skill != "thorns" && e.skill != "dodge"
            && !(e.skill == "poison" && e.type == FightEventAPI.HIT)
            && !(e.skill == "last_stand" && e.type == FightEventAPI.DODGE)
            && !(e.skill == "drain" && e.type == FightEventAPI.HEAL);

        switch (e.type)
        {
            case FightEventAPI.HIT:
                if (e.ByCharacter)
                {
                    bool indirect = e.skill == "poison" || e.skill == "thorns";
                    if (!indirect) Lunge();
                    if (view != null) view.Shake(e.crit ? 16f : 8f);
                    Float(monster + spread, "-" + e.amount + (e.crit ? " !" : ""),
                        e.skill == "poison" ? poisonColor : e.crit ? criticalColor : damageColor, e.crit ? 44 : 34);
                    SoundManager.Play(e.crit ? Sfx.CriticalHit : Sfx.Hit, indirect ? 1.4f : 1f, fightSoundVolume);
                }
                else
                {
                    if (view != null) view.Lunge();
                    if (crewImage != null) StartCoroutine(EncounterView.Punch(crewImage, new Vector2(-8f, 0f), 0.15f));
                    Float(crew + spread, "-" + e.amount, hurtColor, 34);
                    SoundManager.Play(Sfx.Hurt, 1f, fightSoundVolume);
                }
                break;
            case FightEventAPI.HEAL:
                if (e.amount > 0) Float(crew + spread, "+" + e.amount, healColor, 34);
                break;
            case FightEventAPI.DODGE:
                if (view != null) view.Lunge();
                Float(crew + spread, e.skill == "last_stand" ? "Intouchable" : "Esquive", skillColor, 30);
                break;
        }
        if (named)
        {
            Float(crew + new Vector2(0f, 45f), Equipment.SkillName(e.skill), skillColor, 30);
            SoundManager.Play(Sfx.SkillUse, 1f, fightSoundVolume);
        }
        SetHealth(e.hp);
        if (view != null) view.SetHp(e.monsterHp);
    }

    /// <summary>The character died: the expedition is over, and comes back with what was found before.</summary>
    private IEnumerator Die()
    {
        endMiniGame = true;
        expedition.failed = true;
        SetHealth(0);
        SoundManager.Play(Sfx.Defeat);
        Float(CrewPosition() + new Vector2(0f, 60f), "K.O.", hurtColor, 54);
        if (crewImage != null)
        {
            // the character falls on its back
            for (float time = 0f; time < 0.4f; time += Time.deltaTime)
            {
                crewImage.localRotation = Quaternion.Euler(0f, 0f, 90f * time / 0.4f);
                yield return null;
            }
            crewImage.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Image image = crewImage.GetComponent<Image>();
            if (image != null) image.color = new Color(0.6f, 0.6f, 0.6f);
        }
        yield return new WaitForSeconds(1.6f);
        expedition.floorOver = true;
    }

    private void Stop()
    {
        stopped = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void Walk()
    {
        rb.constraints = walkingConstraints;
        stopped = false;
        rb.AddForce(new Vector2(walkSpeed, 0f));
    }

    /// <summary>The character strikes: its image jumps forward.</summary>
    private void Lunge()
    {
        if (crewImage != null) StartCoroutine(EncounterView.Punch(crewImage, new Vector2(18f, 0f), 0.15f));
    }

    private void Float(Vector2 position, string content, Color color, int size)
    {
        EncounterView.FloatText(transform, position, title, content, color, size);
    }

    /// <summary>The position of a transform in this panel, from the center of the panel.</summary>
    private Vector2 PanelPosition(Transform other)
    {
        RectTransform panel = (RectTransform)transform;
        Vector2 local = panel.InverseTransformPoint(other.position);
        // InverseTransformPoint counts from the pivot of the panel
        return local + Vector2.Scale(panel.rect.size, panel.pivot - new Vector2(0.5f, 0.5f));
    }

    private Vector2 CrewPosition()
    {
        return PanelPosition(crewViewport.transform);
    }

    private float CrewX()
    {
        return CrewPosition().x;
    }
}
