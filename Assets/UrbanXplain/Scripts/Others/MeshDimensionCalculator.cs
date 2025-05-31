using UnityEngine;

// This script calculates and logs the dimensions (length, width, height)
// of an assigned Mesh asset based on its local coordinate bounds.
public class MeshDimensionCalculator : MonoBehaviour
{
    // The Mesh asset whose dimensions will be calculated. Assign this in the Inspector.
    public Mesh targetMesh;
    // If true, the dimensions will be calculated and logged when the script starts.
    public bool calculateOnStart = true;

    private void Start()
    {
        // Calculate dimensions on Start if enabled and a mesh is assigned.
        if (calculateOnStart && targetMesh != null)
        {
            CalculateMeshDimensions();
        }
        else if (calculateOnStart && targetMesh == null)
        {
            Debug.LogWarning("MeshDimensionCalculator: 'Calculate On Start' is true, but no Target Mesh is assigned.", this);
        }
    }

    // Calculates the dimensions of the targetMesh and logs them to the console.
    // This method can also be triggered from the component's context menu in the Inspector.
    [ContextMenu("Calculate Mesh Dimensions")]
    public void CalculateMeshDimensions()
    {
        if (targetMesh == null)
        {
            Debug.LogError("MeshDimensionCalculator: No Target Mesh assigned. Cannot calculate dimensions.", this);
            return;
        }

        // Get the bounding box of the mesh in its local coordinates.
        Bounds meshBounds = targetMesh.bounds;

        // Calculate length, width, and height based on the size of the bounds.
        // In Unity's coordinate system by default:
        // X often represents length or width (depending on orientation).
        // Y represents height.
        // Z often represents width or depth (depending on orientation).
        // Here, we assume X for length, Z for width, and Y for height,
        // which is a common interpretation but might need adjustment based on specific model orientation.
        float length = meshBounds.size.x; // Dimension along the local X-axis.
        float width = meshBounds.size.z;  // Dimension along the local Z-axis.
        float height = meshBounds.size.y; // Dimension along the local Y-axis.

        // Log the calculated dimensions.
        Debug.Log($"Mesh Dimensions for '{targetMesh.name}' (Local Coordinates):", this);
        Debug.Log($"Length (X-axis): {length:F2} units", this);
        Debug.Log($"Width (Z-axis): {width:F2} units", this);
        Debug.Log($"Height (Y-axis): {height:F2} units", this);
    }
}