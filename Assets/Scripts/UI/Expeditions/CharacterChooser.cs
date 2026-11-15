using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The window asking which character leads an expedition, shown when the expedition is sent. A character already
/// on an expedition cannot be chosen. The window is built by code, on top of the canvas of the panel opening it.
/// </summary>
public class CharacterChooser : MonoBehaviour
{
    private static CharacterChooser instance;

    private static readonly Vector2 windowSize = new Vector2(980f, 700f);
    private static readonly Vector2 portraitSize = new Vector2(240f, 330f);
    private const float portraitSpacing = 300f;

    private RectTransform list;
    private Text model;
    private Action<GameCharacter> onChosen;

    /// <summary>
    /// Opens the window; onChosen is called with the character clicked, and not at all when the window is closed.
    /// </summary>
    /// <param name="owner">an element of the canvas the window opens in</param>
    /// <param name="model">a text whose font and color are used</param>
    /// <param name="background">sprite of the window, e.g. the one of a panel</param>
    public static void Show(Transform owner, Text model, Sprite background, Action<GameCharacter> onChosen)
    {
        if (instance == null)
        {
            Canvas canvas = owner.GetComponentInParent<Canvas>().rootCanvas;
            RectTransform root = UIBuilder.Child("CharacterChooser", canvas.transform, Vector2.zero, Vector2.one);
            instance = root.gameObject.AddComponent<CharacterChooser>();
            instance.Build(model, background);
        }
        instance.onChosen = onChosen;
        instance.Fill();
        instance.transform.SetAsLastSibling();
        instance.gameObject.SetActive(true);
    }

    private void Build(Text model, Sprite background)
    {
        this.model = model;
        // a click outside the window closes it
        Image blocker = UIBuilder.Image(GetComponent<RectTransform>(), null, new Color(0f, 0f, 0f, 0.5f), true);
        gameObject.AddComponent<Button>().onClick.AddListener(Close);
        GetComponent<Button>().targetGraphic = blocker;
        GetComponent<Button>().transition = Selectable.Transition.None;

        RectTransform window = UIBuilder.Child("Window", transform, new Vector2(0.5f, 0.5f), Vector2.zero, windowSize);
        Image windowImage = UIBuilder.Image(window, background, Color.white, true);
        windowImage.type = Image.Type.Sliced;
        // a click on the window itself must not reach the button behind it, which closes everything
        window.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

        UIBuilder.Text(UIBuilder.Child("Title", window, new Vector2(0.06f, 0.8f), new Vector2(0.94f, 0.95f)),
            model, "Qui part en expédition ?", 64, TextAnchor.MiddleCenter);
        list = UIBuilder.Child("List", window, new Vector2(0f, 0f), new Vector2(1f, 0.8f));
    }

    private void Fill()
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }

        List<GameCharacter> characters = CharacterManager.Instance.characters;
        for (int i = 0; i < characters.Count; i++)
        {
            GameCharacter character = characters[i];
            float x = (i - (characters.Count - 1) / 2f) * portraitSpacing;
            RectTransform entry = UIBuilder.Child("Character" + character.num, list, new Vector2(0.5f, 0.5f),
                new Vector2(x, 40f), portraitSize);
            Image portrait = UIBuilder.Image(entry, character.graphics,
                character.IsAvailable ? Color.white : new Color(1f, 1f, 1f, 0.3f), true);
            portrait.preserveAspect = true;

            Button button = entry.gameObject.AddComponent<Button>();
            button.targetGraphic = portrait;
            button.interactable = character.IsAvailable;
            button.onClick.AddListener(() => Choose(character));

            string label = character.name + (character.IsAvailable ? "" : "\n<color=red>En expédition</color>");
            UIBuilder.Text(UIBuilder.Child("Name", entry, new Vector2(0.5f, 0f), new Vector2(0f, -55f), new Vector2(280f, 100f)),
                model, label, 34, TextAnchor.UpperCenter);
        }
    }

    private void Choose(GameCharacter character)
    {
        Close();
        onChosen?.Invoke(character);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
