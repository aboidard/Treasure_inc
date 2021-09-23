using UnityEngine;
using UnityEngine.UI;

public class ExpeditionCurrentPanel : MonoBehaviour
{
    public GameObject listExpeditions;
    // Start is called before the first frame update

    void Start()
    {
        float width = listExpeditions.GetComponent<RectTransform>().rect.width;
        float height = listExpeditions.GetComponent<RectTransform>().rect.height;
        Vector2 newSize = new Vector2(width, height / 5 - 30);
        listExpeditions.GetComponent<GridLayoutGroup>().cellSize = newSize;
    }
}
