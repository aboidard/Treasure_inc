using UnityEngine;
using UnityEngine.UI;

public class BackgroundGrid : MonoBehaviour
{
    public GameObject grid;
    void Start()
    {
        float width = grid.GetComponent<RectTransform>().rect.width;
        float height = grid.GetComponent<RectTransform>().rect.height;
        Vector2 newSize = new Vector2(width / 11, height / 3);
        grid.GetComponent<GridLayoutGroup>().cellSize = newSize;
    }
}
