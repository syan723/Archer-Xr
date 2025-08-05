using UnityEngine;
using UnityEngine.EventSystems;

public class AxisRotator : MonoBehaviour
{
    public Transform objectToRotate;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 rotationAxis; // Stores the axis to rotate around (e.g., Vector3.right)
    

    private void Update()
    {
        if (isDragging)
        {
            // Calculate the mouse delta
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

            // Determine the rotation angle based on mouse movement.
            // The sensitivity can be adjusted.
            float rotationAmount = 0;
            
            // This is a simplified approach. A more robust solution would project the
            // mouse movement onto a plane perpendicular to the rotation axis.
            if (rotationAxis == Vector3.right || rotationAxis == Vector3.left)
            {
                rotationAmount = mouseDelta.y * 0.5f; // Y mouse movement rotates around X axis
            }
            else if (rotationAxis == Vector3.up || rotationAxis == Vector3.down)
            {
                rotationAmount = mouseDelta.x * 0.5f; // X mouse movement rotates around Y axis
            }
            else if (rotationAxis == Vector3.forward || rotationAxis == Vector3.back)
            {
                rotationAmount = mouseDelta.x * 0.5f; // X mouse movement rotates around Z axis
            }
            
            // Apply the rotation to the object
            // Use Space.Self to rotate the object around its own local axes.
            objectToRotate.Rotate(rotationAxis, rotationAmount, Space.Self);
            
            lastMousePosition = Input.mousePosition;
        }

        // Handle releasing the mouse button
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // This method is called from the individual axis handles.
    public void StartDrag(Vector3 axis)
    {
        isDragging = true;
        rotationAxis = axis;
        lastMousePosition = Input.mousePosition;
    }
}