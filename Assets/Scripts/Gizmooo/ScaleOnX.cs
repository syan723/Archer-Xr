using UnityEngine;

public class ScaleOnX : MonoBehaviour
{
    public Transform target;
    public float scaleSpeed = 0.01f;

    void OnMouseDrag()
    {
        if (target != null)
            target.localScale += new Vector3(0, 0, scaleSpeed);
    }
}