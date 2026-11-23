using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The small window opened by the expedition button of the menu: where to go, the story (the locations, always there)
/// or the events (limited in time or special, given by the server). Built by code, like CharacterChooser.
/// </summary>
public class ExpeditionMenu : MonoBehaviour
{
    private static ExpeditionMenu instance;
    private static readonly Vector2 windowSize = new Vector2(760f, 560f);

    private Action onStory;
    private Action onEvents;

    /// <param name="owner">an element of the canvas the window opens in</param>
    /// <param name="background">sprite of the window, e.g. the one of a panel</param>
    public static void Show(Transform owner, Sprite background, Action onStory, Action onEvents)
    {
        if (instance == null)
        {
            Canvas canvas = owner.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("ExpeditionMenu", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<ExpeditionMenu>();
            instance.Build(background);
        }
        instance.onStory = onStory;
        instance.onEvents = onEvents;
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        SoundManager.Play(Sfx.PanelOpen);
    }

    private void Build(Sprite background)
    {
        // a click outside the window closes it
        Image blocker = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0f, 0f, 0f, 0.5f), true);
        Button close = gameObject.AddComponent<Button>();
        close.targetGraphic = blocker;
        close.transition = Selectable.Transition.None;
        close.onClick.AddListener(Close);

        RectTransform window = UIBuilder.Child("Window", transform, new Vector2(0.5f, 0.5f), Vector2.zero, windowSize);
        Image windowImage = UIBuilder.Image(window, background, Color.white, true);
        windowImage.type = Image.Type.Sliced;
        // a click on the window itself must not reach the blocker behind it
        window.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

        UIBuilder.Text(UIBuilder.Child("Title", window, new Vector2(0.06f, 0.74f), new Vector2(0.94f, 0.93f)),
            UIBuilder.ModelText, "Expéditions", 64, TextAnchor.MiddleCenter);
        UIBuilder.Button(UIBuilder.Child("Story", window, new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.66f)),
            "Histoire", 54, () => Choose(onStory));
        UIBuilder.Button(UIBuilder.Child("Events", window, new Vector2(0.12f, 0.12f), new Vector2(0.88f, 0.36f)),
            "Évènements", 54, () => Choose(onEvents));
    }

    private void Choose(Action action)
    {
        gameObject.SetActive(false);
        action?.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        SoundManager.Play(Sfx.PanelClose);
    }
}
