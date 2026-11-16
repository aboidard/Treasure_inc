using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The window asking which character leads an expedition, shown when the expedition is sent. A character already
/// on an expedition cannot be chosen. The window is built by code, on top of the canvas of the panel opening it.
/// Up to 3 characters stand on one row; a bigger crew is shown smaller, on rows of 5.
/// The row of buttons below the title sorts and filters the characters (CharacterFilterBar); the star of a
/// character makes it a favorite.
/// </summary>
public class CharacterChooser : MonoBehaviour
{
    private static CharacterChooser instance;

    private static readonly Vector2 windowSize = new Vector2(980f, 790f);
    private static readonly Vector2 portraitSize = new Vector2(240f, 330f);
    private const float portraitSpacing = 300f;
    private const float rowSpacing = 470f;
    private const int smallColumns = 5;
    private const float smallScale = 0.6f;
    private const float titleHeight = 320f;
    // from the top of the window: the title, then the sorts and the filters, then the characters
    private const float titleBottom = 140f;
    private const float filtersBottom = 215f;
    private const float listTop = 230f;

    private RectTransform window;
    private RectTransform list;
    private Text model;
    private Action<GameCharacter> onChosen;
    private CharacterListOptions options;

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
        title.offsetMin = new Vector2(0f, -titleBottom);
        title.offsetMax = new Vector2(0f, -35f);
        UIBuilder.Text(title, model, "Qui part en expédition ?", 64, TextAnchor.MiddleCenter);

        options = CharacterListOptions.For("chooser");
        RectTransform filters = UIBuilder.Child("Filters", window, new Vector2(0.04f, 1f), new Vector2(0.96f, 1f));
        filters.offsetMin = new Vector2(0f, -filtersBottom);
        filters.offsetMax = new Vector2(0f, -titleBottom - 10f);
        CharacterFilterBar.Create(filters, options, true, Fill);

        list = UIBuilder.Child("List", window, new Vector2(0f, 0f), new Vector2(1f, 1f));
        list.offsetMax = new Vector2(0f, -listTop);
    }

    private void Fill()
    {
        for (int i = list.childCount - 1; i >= 0; i--)
        {
            Destroy(list.GetChild(i).gameObject);
        }

        List<GameCharacter> characters = options.Apply(CharacterManager.Instance.characters);
        if (characters.Count == 0)
        {
            window.sizeDelta = windowSize;
            UIBuilder.Text(UIBuilder.Child("NoMatch", list, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.7f)),
                model, "Aucun personnage ne correspond aux filtres.", 44, TextAnchor.MiddleCenter);
            return;
        }
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

            // its star: a click makes the character a favorite, or no longer one, without choosing it
            RectTransform star = UIBuilder.Child("Favorite", entry, new Vector2(0.7f, 0.8f), new Vector2(1f, 1f));
            Image starImage = UIBuilder.Image(star, CharacterFilterBar.Star,
                character.IsFavorite ? CharacterFilterBar.FavoriteColor : new Color(1f, 1f, 1f, 0.45f), true);
            starImage.preserveAspect = true;
            Shadow starShadow = star.gameObject.AddComponent<Shadow>();
            starShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
            starShadow.effectDistance = new Vector2(2f, -2f);
            Button favorite = star.gameObject.AddComponent<Button>();
            favorite.targetGraphic = starImage;
            favorite.onClick.AddListener(() =>
            {
                CharacterFavorites.Toggle(character.num);
                Fill();
            });

            string label = character.name + "\n" + CharacterRank.Colored(character.rank) + "   Nv " + character.level
                + (character.IsAvailable ? "" : "\n<color=red>En expédition</color>");
            UIBuilder.Text(UIBuilder.Child("Name", entry, new Vector2(0.5f, 0f), new Vector2(0f, -65f * scale), new Vector2(280f, 120f) * scale),
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
