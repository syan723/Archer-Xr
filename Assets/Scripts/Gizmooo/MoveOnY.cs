using UnityEngine;

public class MoveOnXY: MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 0.05f;

    void OnMouseDrag()
    {
        if (target != null)
            target.position += Vector3.up * moveSpeed;
    }
}