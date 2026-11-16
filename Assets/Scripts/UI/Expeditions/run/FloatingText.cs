using UnityEngine;
using UnityEngine.UI;

/// <summary>Moves its text up while it fades, then destroys it.</summary>
public class FloatingText : MonoBehaviour
{
    private const float duration = 0.9f;
    private const float rise = 70f;

    private float age;
    private Vector2 origin;
    private RectTransform rect;
    private Text text;

    private void Start()
    {
        rect = (RectTransform)transform;
        origin = rect.anchoredPosition;
        text = GetComponent<Text>();
    }

    private void Update()
    {
        age += Time.deltaTime;
        float life = Mathf.Clamp01(age / duration);
        rect.anchoredPosition = origin + new Vector2(0f, rise * (1f - (1f - life) * (1f - life)));
        Color color = text.color;
        color.a = life < 0.6f ? 1f : 1f - (life - 0.6f) / 0.4f;
        text.color = color;
        if (age >= duration) Destroy(gameObject);
    }
}
