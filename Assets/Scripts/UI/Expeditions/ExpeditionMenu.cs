using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The small menu opened by the expedition button of the main menu: the story (the locations, always there) or the
/// events (limited in time or special, given by the server). Same as CrewMenu: it is built by code above the button,
/// on top of the canvas; a click anywhere else closes it.
/// </summary>
public class ExpeditionMenu : MonoBehaviour
{
    private static ExpeditionMenu instance;

    private static readonly Vector2 boxSize = new Vector2(460f, 290f);
    private const float margin = 16f;

    private RectTransform box;
    private Action onStory;
    private Action onEvents;

    /// <summary>Opens the menu above the button, or closes it when it is open.</summary>
    public static void Toggle(RectTransform button, Action onStory, Action onEvents)
    {
        if (instance != null && instance.gameObject.activeSelf)
        {
            instance.Close();
            return;
        }
        if (instance == null)
        {
            Canvas canvas = button.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("ExpeditionMenu", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<ExpeditionMenu>();
            instance.Build();
        }
        instance.onStory = onStory;
        instance.onEvents = onEvents;
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
        instance.Place(button);
        SoundManager.Play(Sfx.PanelOpen, 1.3f);
    }

    private void Build()
    {
        // invisible, but it takes the clicks made outside of the menu
        Image blocker = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0f, 0f, 0f, 0f), true);
        Button outside = gameObject.AddComponent<Button>();
        outside.targetGraphic = blocker;
        outside.transition = Selectable.Transition.None;
        outside.onClick.AddListener(Close);

        box = UIBuilder.Child("Box", transform, new Vector2(0.5f, 0.5f), Vector2.zero, boxSize);
        Image frame = UIBuilder.Image(box, UIBuilder.ModelPanel.sprite, Color.white, true);
        frame.type = UIBuilder.ModelPanel.type;

        UIBuilder.Button(UIBuilder.Child("Story", box, new Vector2(0.08f, 0.54f), new Vector2(0.92f, 0.9f)),
            "Histoire", 52, () => Choose(onStory));
        UIBuilder.Button(UIBuilder.Child("Events", box, new Vector2(0.08f, 0.1f), new Vector2(0.92f, 0.46f)),
            "Évènements", 52, () => Choose(onEvents));
    }

    /// <summary>Just above the button, without leaving the screen.</summary>
    private void Place(RectTransform button)
    {
        RectTransform root = GetComponent<RectTransform>();
        Vector3[] corners = new Vector3[4];
        button.GetWorldCorners(corners);
        Vector2 topLeft = root.InverseTransformPoint(corners[1]);
        Vector2 topRight = root.InverseTransformPoint(corners[2]);
        Rect area = root.rect;

        float x = (topLeft.x + topRight.x) / 2f;
        float y = topLeft.y + margin + boxSize.y / 2f;
        float xLimit = Mathf.Max(0f, area.width / 2f - boxSize.x / 2f - margin);
        float yLimit = Mathf.Max(0f, area.height / 2f - boxSize.y / 2f - margin);
        x = Mathf.Clamp(x, area.center.x - xLimit, area.center.x + xLimit);
        y = Mathf.Clamp(y, area.center.y - yLimit, area.center.y + yLimit);
        box.localPosition = new Vector3(x, y, 0f);
    }

    private void Choose(Action action)
    {
        gameObject.SetActive(false);
        action?.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
