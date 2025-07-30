using UnityEngine;

public class RotateOnY : MonoBehaviour
{
    public Transform target;
    public float rotationSpeed = 50f;

    void OnMouseDrag()
    {
        if (target != null)
            target.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}