using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpeditionGridPanel : MonoBehaviour
{
    public const int SIZE_S = 1;
    public const int SIZE_M = 2;
    public const int SIZE_L = 3;
    public const int SIZE_XL = 4;
    public const int SIZE_XXL = 5;

    public List<GameObject> positions;
    public static ExpeditionGridPanel instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("plus d'une instance de " + this.GetType().Name + " dans la scène");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        positions.ForEach(delegate (GameObject position)
        {
            if (position.transform.childCount > 0) Destroy(position.transform.GetChild(0).gameObject);
        });

    }

    public bool isFreeSpot(int size)
    {
        int freeSpot = 0;
        positions.ForEach(delegate (GameObject position)
        {
            if (position.transform.childCount == 0) freeSpot++;
        });
        return freeSpot >= size ? true : false;
    }

    public GameObject GetConsecutiveFreeSpot(int size)
    {
        if (size == 0) return null;

        int consecutiveFreeSpot = 0;
        int maxConsecutiveFreeSpot = 0;
        GameObject spot = null;

        positions.ForEach(delegate (GameObject position)
        {
            if (position.transform.childCount == 0)
            {
                consecutiveFreeSpot++;
                if (maxConsecutiveFreeSpot < consecutiveFreeSpot)
                {
                    maxConsecutiveFreeSpot = consecutiveFreeSpot;
                }
            }
            else
            {
                consecutiveFreeSpot = 0;
            }
        });
        Debug.Log("maxConsecutiveFreeSpot : " + maxConsecutiveFreeSpot);
        return maxConsecutiveFreeSpot >= size ? spot : null;
    }

    public GameObject GetFreeSpot(int size)
    {
        GameObject spot = null;
        positions.ForEach(delegate (GameObject position)
        {
            if (position.transform.childCount == 0) spot = position;
        });
        Debug.Log("freespot : " + spot);
        return spot;
    }

    public void sort()
    {

    }
}
