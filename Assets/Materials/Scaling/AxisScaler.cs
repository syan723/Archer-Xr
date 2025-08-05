using UnityEngine;
using UnityEngine.EventSystems;

public class AxisScaler : MonoBehaviour
{
    public Transform objectToScale;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private Vector3 scaleAxis; // Stores the axis(es) to scale along



    private void Update()
    {
        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // Calculate the scaling amount based on the mouse movement.
            // You can adjust the sensitivity by changing the multiplier.
            float scaleAmount = (mouseDelta.x + mouseDelta.y) * 0.01f;

            // Apply the scaling.
            // We use a temporary Vector3 to avoid modifying the localScale directly in the calculation.
            Vector3 newScale = objectToScale.localScale;
            
            if (scaleAxis == Vector3.one)
            {
                // Uniform scaling: apply the change to all axes.
                newScale += Vector3.one * scaleAmount;
            }
            else
            {
                // Single-axis scaling: apply the change to the selected axis.
                newScale += scaleAxis * scaleAmount;
            }
            
            // Ensure scale doesn't go to zero or negative values.
            newScale.x = Mathf.Max(newScale.x, 0.1f);
            newScale.y = Mathf.Max(newScale.y, 0.1f);
            newScale.z = Mathf.Max(newScale.z, 0.1f);

            objectToScale.localScale = newScale;

            lastMousePosition = Input.mousePosition;
        }

        // Handle releasing the mouse button
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    // This method is called from the individual scale handles.
    public void StartDrag(Vector3 axis)
    {
        isDragging = true;
        scaleAxis = axis;
        lastMousePosition = Input.mousePosition;
    }
}