using UnityEngine;

public class MoveOnX : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 0.05f;

    void OnMouseDrag()
    {
        if (target != null)
            target.position += Vector3.right * moveSpeed;
    }
}