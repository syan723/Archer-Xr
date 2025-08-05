using UnityEngine;

public class ScaleHandle : MonoBehaviour
{
    // Public variable to set the axis of scaling in the Inspector.
    // Use (1, 0, 0) for X, (0, 1, 0) for Y, (0, 0, 1) for Z, and (1, 1, 1) for uniform.
    public Vector3 axisToScale = Vector3.one; 
    private AxisScaler parentScaler;

    private void Start()
    {
        parentScaler = GetComponentInParent<AxisScaler>();
    }

    private void OnMouseDown()
    {
        if (parentScaler != null)
        {
            parentScaler.StartDrag(axisToScale);
        }
    }
}