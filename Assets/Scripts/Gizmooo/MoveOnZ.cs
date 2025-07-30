using UnityEngine;

public class MoveOnZ : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 0.05f;

    void OnMouseDrag()
    {
        if (target != null)
            target.position += Vector3.forward * moveSpeed;
    }
}