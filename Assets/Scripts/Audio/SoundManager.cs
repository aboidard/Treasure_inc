using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>The sound effects of the game; SoundSynth has the recipe of each of them.</summary>
public enum Sfx
{
    // interface
    Click, Tick, Pop, PanelOpen, PanelClose, Error, GameStart,
    // money and items
    Coins, Equip, Unequip, ChestOpen, ItemsFound, RareItem,
    // expeditions
    Dig, DigNothing, DigFound, ExpeditionSend, ExpeditionReturn, Step, EventReach,
    // recruitment
    PortalOpen, SummonStep, SummonBurst, RankStamp, RevealCommon, RevealRare, RevealEpic, RevealLegend,
}

/// <summary>
/// Plays the sound effects: SoundManager.Play(Sfx.Coins). It lives through the scenes and needs nothing in them.
///   - the sounds are computed by SoundSynth the first time; an audio file named like a sound in a
///     "Resources/Sounds" folder (e.g. Resources/Sounds/Coins.wav) replaces the computed one;
///   - every button, toggle or slider clicks when it is pressed, without any code in the screens;
///   - the user can turn the sounds off (Muted, kept in the preferences; see SettingsPanel).
/// </summary>
public class SoundManager : MonoBehaviour
{
    private const string mutedKey = "soundMuted";
    private const int voices = 8;
    private const float masterVolume = 0.8f;
    // the same sound asked again within this time is played once (e.g. several expeditions coming back together)
    private const float sameSoundDelay = 0.05f;

    private static SoundManager _Instance;

    private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    private readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();
    private readonly List<RaycastResult> hits = new List<RaycastResult>();
    private AudioSource[] sources;
    private AudioListener listener;
    private int nextSource;
    private bool muted;
    private Selectable pressed;
    private Vector2 pressedAt;

    public static SoundManager Instance
    {
        get
        {
            if (!_Instance)
            {
                _Instance = new GameObject("SoundManager").AddComponent<SoundManager>();
                DontDestroyOnLoad(_Instance.gameObject);
            }
            return _Instance;
        }
    }

    // created with the game, so that the buttons click from the first screen
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateWithTheGame()
    {
        Instance.EnsureListener();
    }

    /// <summary>Plays a sound; pitch 2 is one octave higher, volume goes from 0 to 1.</summary>
    public static void Play(Sfx sfx, float pitch = 1f, float volume = 1f)
    {
        Instance.PlayNow(sfx, pitch, volume);
    }

    /// <summary>No sound is played while true; the choice is kept for the next sessions.</summary>
    public bool Muted
    {
        get => muted;
        set
        {
            muted = value;
            PlayerPrefs.SetInt(mutedKey, muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    void Awake()
    {
        muted = PlayerPrefs.GetInt(mutedKey, 0) == 1;
        sources = new AudioSource[voices];
        for (int i = 0; i < voices; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
            sources[i].spatialBlend = 0f;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(Prepare());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureListener();
    }

    /// <summary>A scene without a camera that listens would stay silent: the manager then listens itself.</summary>
    private void EnsureListener()
    {
        if (listener == null)
        {
            listener = gameObject.AddComponent<AudioListener>();
        }
        bool sceneListens = false;
        foreach (AudioListener other in FindObjectsOfType<AudioListener>())
        {
            if (other != listener && other.enabled) sceneListens = true;
        }
        listener.enabled = !sceneListens;
    }

    /// <summary>Computes the sounds a few at a time after the start, so that none is computed while playing.</summary>
    private IEnumerator Prepare()
    {
        foreach (Sfx sfx in System.Enum.GetValues(typeof(Sfx)))
        {
            Clip(sfx);
            yield return null;
        }
    }

    private AudioClip Clip(Sfx sfx)
    {
        if (clips.TryGetValue(sfx, out AudioClip known) && known != null) return known;
        AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sfx);
        if (clip == null) clip = SoundSynth.Build(sfx);
        clips[sfx] = clip;
        return clip;
    }

    private void PlayNow(Sfx sfx, float pitch, float volume)
    {
        if (muted) return;
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(sfx, out float last) && now - last < sameSoundDelay) return;
        lastPlayed[sfx] = now;

        // the voices are taken in turn: the oldest sound is cut when they all play
        AudioSource source = sources[nextSource];
        nextSource = (nextSource + 1) % sources.Length;
        source.Stop();
        source.clip = Clip(sfx);
        source.pitch = Mathf.Clamp(pitch, 0.25f, 3f);
        source.volume = Mathf.Clamp01(volume) * masterVolume;
        source.Play();
    }

    /// <summary>
    /// The click of the interface: a button, a toggle or a slider pressed then released without moving away, which
    /// is what the interface takes for a click (dragging a list does not click).
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressed = SelectableAt(Input.mousePosition);
            pressedAt = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0) && pressed != null)
        {
            float moved = Vector2.Distance(pressedAt, Input.mousePosition);
            float allowed = EventSystem.current != null ? EventSystem.current.pixelDragThreshold * 2f : 20f;
            if (moved <= allowed && SelectableAt(Input.mousePosition) == pressed)
            {
                PlayNow(Sfx.Click, 1f, 1f);
            }
            pressed = null;
        }
    }

    private Selectable SelectableAt(Vector2 position)
    {
        EventSystem system = EventSystem.current;
        if (system == null) return null;
        PointerEventData pointer = new PointerEventData(system) { position = position };
        hits.Clear();
        system.RaycastAll(pointer, hits);
        if (hits.Count == 0) return null;
        Selectable selectable = hits[0].gameObject.GetComponentInParent<Selectable>();
        return selectable != null && selectable.IsInteractable() ? selectable : null;
    }
}
