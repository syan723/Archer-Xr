using UnityEngine;
using UnityEngine.EventSystems;

public class AxisMover : MonoBehaviour
{
    public Transform objectToMove;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 movementAxis; // Stores the axis to move along (e.g., Vector3.right for X axis)

 

    private void Update()
    {
        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

            // Project the mouse movement onto the chosen axis to get the movement amount.
            // We use the screen space direction of the mouse movement.
            Vector3 screenMovementAxis = Camera.main.WorldToScreenPoint(objectToMove.position + movementAxis) - Camera.main.WorldToScreenPoint(objectToMove.position);
            screenMovementAxis.Normalize();

            float movementAmount = Vector3.Dot(mouseDelta, screenMovementAxis) * Time.deltaTime * 5f; // Adjust multiplier for sensitivity.

            // Move the object along its local axis.
            objectToMove.Translate(movementAxis * movementAmount, Space.Self);
            
            lastMousePosition = Input.mousePosition;
        }

        // Handle releasing the mouse button
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // This method is called from the individual axis cubes.
    // It's a simple way to start the drag and set the axis.
    public void StartDrag(Vector3 axis)
    {
        isDragging = true;
        movementAxis = axis;
        lastMousePosition = Input.mousePosition;
    }
}