using UnityEngine;

public class AxisHandle : MonoBehaviour
{
    public Vector3 axisToMove = Vector3.right; // Set this in the Inspector for each cube.
    private AxisMover parentMover;

    private void Start()
    {
        // Find the parent AxisMover script.
        parentMover = GetComponentInParent<AxisMover>();
    }

    private void OnMouseDown()
    {
        if (parentMover != null)
        {
            parentMover.StartDrag(axisToMove);
        }
    }
}