using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The window asking which character leads an expedition, shown when the expedition is sent. A character already
/// on an expedition cannot be chosen. The window is built by code, on top of the canvas of the panel opening it.
/// Up to 3 characters stand on one row; a bigger crew is shown smaller, on rows of 5.
/// </summary>
public class CharacterChooser : MonoBehaviour
{
    private static CharacterChooser instance;

    private static readonly Vector2 windowSize = new Vector2(980f, 700f);
    private static readonly Vector2 portraitSize = new Vector2(240f, 330f);
    private const float portraitSpacing = 300f;
    private const float rowSpacing = 470f;
    private const int smallColumns = 5;
    private const float smallScale = 0.6f;
    private const float titleHeight = 230f;

    private RectTransform window;
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

        window = UIBuilder.Child("Window", transform, new Vector2(0.5f, 0.5f), Vector2.zero, windowSize);
        Image windowImage = UIBuilder.Image(window, background, Color.white, true);
        windowImage.type = Image.Type.Sliced;
        // a click on the window itself must not reach the button behind it, which closes everything
        window.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;

        // the title keeps its height at the top of the window, whose height depends on the number of characters
        RectTransform title = UIBuilder.Child("Title", window, new Vector2(0.06f, 1f), new Vector2(0.94f, 1f));
        title.offsetMin = new Vector2(0f, -140f);
        title.offsetMax = new Vector2(0f, -35f);
        UIBuilder.Text(title, model, "Qui part en expédition ?", 64, TextAnchor.MiddleCenter);
        list = UIBuilder.Child("List", window, new Vector2(0f, 0f), new Vector2(1f, 1f));
        list.offsetMax = new Vector2(0f, -140f);
    }

    private void Fill()
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }

        List<GameCharacter> characters = CharacterManager.Instance.characters;
        bool small = characters.Count > 3;
        int columns = small ? smallColumns : Mathf.Max(1, characters.Count);
        float scale = small ? smallScale : 1f;
        int rows = Mathf.Max(1, Mathf.CeilToInt(characters.Count / (float)columns));
        window.sizeDelta = new Vector2(windowSize.x, small ? titleHeight + rows * rowSpacing * scale : windowSize.y);

        for (int i = 0; i < characters.Count; i++)
        {
            GameCharacter character = characters[i];
            int row = i / columns;
            int inRow = Mathf.Min(columns, characters.Count - row * columns);
            float x = (i % columns - (inRow - 1) / 2f) * portraitSpacing * scale;
            float y = ((rows - 1) / 2f - row) * rowSpacing * scale + 40f * scale;
            RectTransform entry = UIBuilder.Child("Character" + character.num, list, new Vector2(0.5f, 0.5f),
                new Vector2(x, y), portraitSize * scale);
            Image portrait = UIBuilder.Image(entry, character.graphics,
                character.IsAvailable ? Color.white : new Color(1f, 1f, 1f, 0.3f), true);
            portrait.preserveAspect = true;

            Button button = entry.gameObject.AddComponent<Button>();
            button.targetGraphic = portrait;
            button.interactable = character.IsAvailable;
            button.onClick.AddListener(() => Choose(character));

            string label = character.name + (character.IsAvailable ? "" : "\n<color=red>En expédition</color>");
            UIBuilder.Text(UIBuilder.Child("Name", entry, new Vector2(0.5f, 0f), new Vector2(0f, -55f * scale), new Vector2(280f, 100f) * scale),
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
