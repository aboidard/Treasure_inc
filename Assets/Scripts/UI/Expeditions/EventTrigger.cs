using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
    public bool isInRange;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Crew"))
        {
            isInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Crew"))
        {
            isInRange = false;
        }
    }
}
