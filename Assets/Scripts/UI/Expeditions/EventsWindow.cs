using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// The events open now, read from the server each time the window opens: expeditions limited in time or special,
/// changed from Maestro. Each one shows its picture, its name, its difficulty in stars, its floors, its rewards and
/// what the player may still do there today; "Partir" asks which character goes, and sends it for free.
/// Built by code, like CharacterChooser.
/// </summary>
public class EventsWindow : MonoBehaviour
{
    private static EventsWindow instance;
    private static readonly Vector2 windowSize = new Vector2(1100f, 1300f);
    private const float entryHeight = 330f;
    /// <summary>The pictures of the events, by their image (sprite name or URL), once found.</summary>
    private static readonly Dictionary<string, Sprite> pictures = new Dictionary<string, Sprite>();

    private RectTransform list;
    private Text status;
    private Sprite background;

    public static void Show()
    {
        if (instance == null)
        {
            Transform owner = SendExpeditionPanel.instance.transform;
            Canvas canvas = owner.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("EventsWindow", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<EventsWindow>();
            instance.Build(SendExpeditionPanel.instance.panel.GetComponent<Image>().sprite);
        }
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        SoundManager.Play(Sfx.PanelOpen);
        instance.StartCoroutine(instance.Load());
    }

    /// <summary>
    /// The location an expedition to the event shows on its way: its name, its picture (that of the first location of
    /// the story while it is not known), its level.
    /// </summary>
    public static Location LocationOf(GameEventAPI ev)
    {
        Location location = ScriptableObject.CreateInstance<Location>();
        location.id = 0;
        location.name = ev.name;
        location.level = ev.level;
        location.graphics = ev.image != null && pictures.TryGetValue(ev.image, out Sprite picture) ? picture : FallbackPicture();
        return location;
    }

    private static Sprite FallbackPicture()
    {
        var locations = LocationsDatabase.instance != null ? LocationsDatabase.instance.allLocations : null;
        return locations != null && locations.Length > 0 ? locations[0].graphics : null;
    }

    private void Build(Sprite background)
    {
        this.background = background;
        Image blocker = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0f, 0f, 0f, 0.5f), true);
        Button close = gameObject.AddComponent<Button>();
        close.targetGraphic = blocker;
        close.transition = Selectable.Transition.None;
        close.onClick.AddListener(Close);

        RectTransform window = UIBuilder.Child("Window", transform, new Vector2(0.5f, 0.5f), Vector2.zero, windowSize);
        Image windowImage = UIBuilder.Image(window, background, Color.white, true);
        windowImage.type = Image.Type.Sliced;
        window.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

        UIBuilder.Text(UIBuilder.Child("Title", window, new Vector2(0.06f, 0.9f), new Vector2(0.94f, 0.97f)),
            UIBuilder.ModelText, "Évènements", 64, TextAnchor.MiddleCenter);

        // the list scrolls when there are more events than the window holds
        RectTransform viewport = UIBuilder.Child("Viewport", window, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.89f));
        UIBuilder.Image(viewport, null, new Color(1f, 1f, 1f, 0.01f), true);
        viewport.gameObject.AddComponent<RectMask2D>();
        list = UIBuilder.Child("List", viewport, new Vector2(0f, 1f), new Vector2(1f, 1f));
        list.pivot = new Vector2(0.5f, 1f);
        VerticalLayoutGroup layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandWidth = true;
        list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.content = list;
        scroll.horizontal = false;
        scroll.viewport = viewport;

        status = UIBuilder.Text(UIBuilder.Child("Status", window, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f)),
            UIBuilder.ModelText, "", 44, TextAnchor.MiddleCenter);
    }

    private IEnumerator Load()
    {
        Clear();
        status.text = "Chargement...";
        long code = 0;
        string response = null;
        yield return NetworkManager.Instance.GetJson(NetworkManager.eventsEndPoint, (c, text) =>
        {
            code = c;
            response = text;
        });
        if (code != 200)
        {
            status.text = "Impossible de charger les évènements, veuillez réessayer ultérieurement !";
            yield break;
        }
        GameEventsAPI read = JsonConvert.DeserializeObject<GameEventsAPI>(response);
        List<GameEventAPI> events = read?.events ?? new List<GameEventAPI>();
        status.text = events.Count == 0 ? "Aucun évènement en ce moment : revenez bientôt !" : "";
        foreach (GameEventAPI ev in events)
        {
            Add(ev);
        }
    }

    private void Clear()
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }
    }

    private void Add(GameEventAPI ev)
    {
        RectTransform entry = UIBuilder.Child("Event_" + ev.code, list, new Vector2(0f, 1f), new Vector2(1f, 1f));
        entry.sizeDelta = new Vector2(0f, entryHeight);
        Image frame = UIBuilder.Image(entry, background, new Color(1f, 1f, 1f, 0.9f), false);
        frame.type = Image.Type.Sliced;
        Text model = UIBuilder.ModelText;

        Image picture = UIBuilder.Image(UIBuilder.Child("Picture", entry, new Vector2(0.03f, 0.1f), new Vector2(0.27f, 0.9f)),
            FallbackPicture(), Color.white, false);
        picture.preserveAspect = true;
        StartCoroutine(LoadPicture(ev.image, picture));

        UIBuilder.FitOneLine(UIBuilder.Text(UIBuilder.Child("Name", entry, new Vector2(0.3f, 0.74f), new Vector2(0.72f, 0.92f)),
            model, ev.name, 46, TextAnchor.MiddleLeft));
        DifficultyStars.Create(entry, new Vector2(0.3f, 0.58f), new Vector2(0.48f, 0.72f), ev.level);
        UIBuilder.Text(UIBuilder.Child("Difficulty", entry, new Vector2(0.49f, 0.58f), new Vector2(0.72f, 0.72f)),
            model, DifficultyStars.Label(ev.level) + " · " + ev.floors + (ev.floors > 1 ? " étages" : " étage"), 30, TextAnchor.MiddleLeft);
        if (!string.IsNullOrEmpty(ev.description))
        {
            UIBuilder.Text(UIBuilder.Child("Description", entry, new Vector2(0.3f, 0.3f), new Vector2(0.97f, 0.56f)),
                model, ev.description, 28, TextAnchor.UpperLeft);
        }
        UIBuilder.Text(UIBuilder.Child("Rewards", entry, new Vector2(0.3f, 0.08f), new Vector2(0.72f, 0.28f)),
            model, Rewards(ev.rewards), 32, TextAnchor.MiddleLeft);

        bool done = ev.remainingToday.HasValue && ev.remainingToday.Value <= 0;
        Button go = UIBuilder.Button(UIBuilder.Child("Go", entry, new Vector2(0.74f, 0.62f), new Vector2(0.97f, 0.9f)),
            done ? "Demain !" : "Partir", 40, () => Go(ev));
        go.interactable = !done;
        UIBuilder.Text(UIBuilder.Child("Left", entry, new Vector2(0.72f, 0.06f), new Vector2(0.98f, 0.28f)),
            model, Remaining(ev), 26, TextAnchor.MiddleCenter);
    }

    private static string Rewards(GameEventRewardsAPI rewards)
    {
        if (rewards == null) return "";
        List<string> parts = new List<string>();
        if (rewards.gems > 0) parts.Add("<color=#" + ColorUtility.ToHtmlStringRGB(Gems.TextColor) + ">" + Gems.Amount(rewards.gems) + "</color>");
        if (rewards.money > 0) parts.Add(UIBuilder.Number(rewards.money) + " pièces");
        if (rewards.items > 0) parts.Add(rewards.items + (rewards.items > 1 ? " objets" : " objet"));
        return parts.Count == 0 ? "" : "Gains : " + string.Join(", ", parts);
    }

    private static string Remaining(GameEventAPI ev)
    {
        string text = ev.remainingToday.HasValue
            ? (ev.remainingToday.Value <= 0 ? "Fait aujourd'hui" : ev.remainingToday.Value + "/" + ev.maxPerDay + " aujourd'hui")
            : "Sans limite";
        if (!string.IsNullOrEmpty(ev.end) && DateTime.TryParse(ev.end, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime end))
        {
            text += "\njusqu'au " + end.ToLocalTime().ToString("dd/MM HH:mm");
        }
        return text;
    }

    /// <summary>
    /// The picture of an event: a sprite of Resources/Events, or a picture downloaded from its URL; the picture of the
    /// first location of the story stays when neither is found.
    /// </summary>
    private static IEnumerator LoadPicture(string image, Image target)
    {
        if (string.IsNullOrEmpty(image)) yield break;
        if (pictures.TryGetValue(image, out Sprite known))
        {
            target.sprite = known;
            yield break;
        }
        Sprite found = null;
        if (image.StartsWith("http://") || image.StartsWith("https://"))
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(image))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    found = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogWarning("event picture not loaded: " + image + " (" + request.error + ")");
                }
            }
        }
        else
        {
            found = Resources.Load<Sprite>("Events/" + image);
        }
        if (found == null) yield break;
        pictures[image] = found;
        if (target != null) target.sprite = found;
    }

    private void Go(GameEventAPI ev)
    {
        if (ExpeditionManager.instance.isSending) return;
        if (!SendExpeditionPanel.instance.CheckCharacterAvailable()) return;

        CharacterChooser.Show(transform, UIBuilder.ModelText, background, character =>
        {
            if (ExpeditionManager.instance.isSending) return;
            try
            {
                ExpeditionManager.instance.SendEventExpedition(ev, character, () =>
                {
                    gameObject.SetActive(false);
                });
            }
            catch (TooMuchExpeditionsException)
            {
                MessagePanel.instance.DisplayMessage("Trop d'expéditions", "Attendez le retour d'une expédition avant d'en envoyer une autre.");
            }
        });
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SoundManager.Play(Sfx.PanelClose);
    }
}
