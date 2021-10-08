using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public const int POSITION_SEND = 0;
    public const int POSITION_INVENTORY = 1;
    public const int POSITION_CREW = 2;
    public const int POSITION_SETTINGS = 3;
    public GameObject[] Positions;
    public int currentPosition = POSITION_SEND;
    public float timeOffset;

    public Vector3 posOffset;

    private Vector3 velocity;

    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, Positions[currentPosition].transform.position + posOffset, ref velocity, timeOffset);
    }

    public void UpdatePosition(int position)
    {
        currentPosition = position;
    }
}
