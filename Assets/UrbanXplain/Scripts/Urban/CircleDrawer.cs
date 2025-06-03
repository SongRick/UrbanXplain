using UnityEngine;

namespace UrbanXplain
{
    // This component draws a circle using a LineRenderer.
    // It allows customization of segments, radius, y-offset, line width, and color.
    // It handles material instantiation appropriately for runtime and editor contexts.
    [RequireComponent(typeof(LineRenderer))]
    public class CircleDrawer : MonoBehaviour
    {
        [Header("Circle Settings")]
        // Number of line segments used to approximate the circle. More segments result in a smoother circle.
        [Range(3, 100)]
        public int segments = 50;
        // The radius of the circle.
        public float radius = 10f;
        // The vertical offset of the circle from the GameObject's local y=0 plane.
        public float yOffset = 0.1f;

        [Header("Line Settings")]
        // The width of the line used to draw the circle.
        public float lineWidth = 0.2f;
        // The color of the line.
        public Color lineColor = Color.green;

        // (Optional) A material template can be provided. If not, a default material will be attempted.
        [Header("Material (Optional)")]
        public Material lineMaterialTemplate;

        // Cached reference to the LineRenderer component.
        private LineRenderer lineRenderer;
        // Holds the runtime instance of the material to ensure independent modifications.
        private Material _instancedMaterial;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            EnsureMaterialIsInstanced(); // Ensure a unique material instance is used at runtime.
            SetupCircle(); // Draw the circle with initial settings.
        }

        // Called in the editor when the script is loaded or a value is changed in the Inspector.
        // This ensures the circle preview updates in the editor.
        void OnValidate()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
            // SetupCircle handles material logic for editor preview and runtime.
            SetupCircle();
        }

        // Ensures that the LineRenderer uses an instanced material at runtime.
        // This prevents modifications to the material asset itself if a template is used,
        // or creates a default material instance if none is provided.
        void EnsureMaterialIsInstanced()
        {
            if (lineRenderer == null) return;

            if (Application.isPlaying) // Only perform instantiation logic at runtime.
            {
                if (lineMaterialTemplate != null)
                {
                    // If a template is provided, create an instance of it.
                    _instancedMaterial = new Material(lineMaterialTemplate);
                    lineRenderer.material = _instancedMaterial;
                }
                // If no template, and current material is shared or not an instance, create a default instance.
                else if (lineRenderer.sharedMaterial == null || !lineRenderer.sharedMaterial.name.EndsWith("(Instance)"))
                {
                    _instancedMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                    _instancedMaterial.name = "CircleLine_RuntimeInstance"; // Name the instance for clarity.
                    lineRenderer.material = _instancedMaterial;
                }
                // If it's already a runtime instance (e.g., from accessing .material).
                else if (lineRenderer.material != null && lineRenderer.material.name.EndsWith("(Instance)"))
                {
                    _instancedMaterial = lineRenderer.material;
                }
                // If a shared material exists, accessing .material will create an instance.
                else if (lineRenderer.sharedMaterial != null)
                {
                    _instancedMaterial = lineRenderer.material; // This access creates/retrieves the instance.
                }

                // If an instanced material was successfully obtained or created, apply the line color.
                if (_instancedMaterial != null)
                {
                    _instancedMaterial.color = lineColor;
                }
            }
        }

        // Configures the LineRenderer to draw the circle based on current settings.
        // This method handles both runtime and editor-time setup, including material and vertex calculation.
        public void SetupCircle()
        {
            if (lineRenderer == null)
            {
                // Debug.LogError("LineRenderer component not found on " + gameObject.name + "!");
                return;
            }

            // Configure basic LineRenderer properties.
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = segments + 1; // +1 to close the circle loop.
            lineRenderer.useWorldSpace = false;       // Draw in local space relative to the GameObject.

            // --- Material and Color Handling ---
            if (Application.isPlaying) // Runtime logic
            {
                // EnsureMaterialIsInstanced should have been called in Awake.
                // This is a safeguard if SetupCircle is called from elsewhere during runtime.
                if (_instancedMaterial == null)
                {
                    EnsureMaterialIsInstanced();
                }

                if (_instancedMaterial != null)
                {
                    _instancedMaterial.color = lineColor; // Apply color to the runtime instance.
                    lineRenderer.material = _instancedMaterial; // Ensure the instanced material is assigned.
                }
                // Fallback if _instancedMaterial somehow became null (should not happen if Awake ran).
                else if (lineRenderer.material != null)
                {
                    lineRenderer.material.color = lineColor;
                }
            }
            else // Editor mode logic (typically from OnValidate)
            {
                Material currentMaterialToUse = lineRenderer.sharedMaterial; // Start with sharedMaterial for editor.

                if (currentMaterialToUse == null) // If no shared material is assigned.
                {
                    if (lineMaterialTemplate != null)
                    {
                        // Use the provided template as the shared material.
                        lineRenderer.sharedMaterial = lineMaterialTemplate;
                        currentMaterialToUse = lineMaterialTemplate;
                        // Modifying color here will affect the template asset itself.
                        currentMaterialToUse.color = lineColor;
                    }
                    else
                    {
                        // No template and no shared material. For editor preview, we might need to create a temporary instance.
                        // This is done by assigning to .material, which can log an instantiation warning in editor.
                        // Check if lineRenderer.material already has a suitable preview material.
                        if (lineRenderer.material == null || lineRenderer.material.shader.name != "Legacy Shaders/Particles/Alpha Blended Premultiply")
                        {
                            //Debug.LogWarning("CircleDrawer: No material assigned and no template for editor preview. Creating temporary preview material. This might log an instantiation warning if done frequently.", this);
                            Material tempEditorMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
                            tempEditorMaterial.name = "CircleLine_EditorPreviewInstance"; // Mark as a preview instance.
                            lineRenderer.material = tempEditorMaterial; // Assigning to .material creates an instance for this renderer.
                            currentMaterialToUse = tempEditorMaterial;
                        }
                        else
                        {
                            currentMaterialToUse = lineRenderer.material; // Reuse existing temporary instance.
                        }

                        if (currentMaterialToUse != null)
                        {
                            currentMaterialToUse.color = lineColor;
                        }
                    }
                }
                else // A shared material is already assigned.
                {
                    // Modify the color of the shared material (this will affect the material asset).
                    currentMaterialToUse.color = lineColor;
                    // Ensure the LineRenderer is still using this shared material (usually it is).
                    lineRenderer.sharedMaterial = currentMaterialToUse;
                }
            }

            // --- Vertex Calculation ---
            // Calculate the angle increment for each segment.
            float angleIncrement = 360f / segments;
            // Generate vertex positions for the circle.
            for (int i = 0; i <= segments; i++) // Iterate one extra time to close the loop.
            {
                float angle = i * angleIncrement * Mathf.Deg2Rad; // Convert angle to radians.
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, yOffset, z));
            }
        }

        // Public method to update the circle's properties dynamically.
        // This is useful if you need to change the circle's appearance from another script.
        public void UpdateCircle(float newRadius, Color newColor, float newLineWidth)
        {
            radius = newRadius;
            lineColor = newColor;
            lineWidth = newLineWidth;

            // Ensure color is updated on the instanced material if at runtime.
            if (Application.isPlaying && _instancedMaterial != null)
            {
                _instancedMaterial.color = lineColor;
            }
            // Re-setup the circle with the new parameters.
            SetupCircle();
        }
    }
}