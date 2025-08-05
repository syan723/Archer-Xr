using UnityEngine;

public class RotationHandle : MonoBehaviour
{
    // Public variable to set the axis of rotation in the Inspector.
    public Vector3 axisToRotate = Vector3.up; 
    private AxisRotator parentRotator;

    private void Start()
    {
        // Find the parent AxisRotator script.
        parentRotator = GetComponentInParent<AxisRotator>();
    }

    private void OnMouseDown()
    {
        if (parentRotator != null)
        {
            parentRotator.StartDrag(axisToRotate);
        }
    }
}