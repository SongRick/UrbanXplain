using UnityEngine;
using System.Collections.Generic;

namespace UrbanXplain
{
    // Manages the color of renderer materials on this GameObject and its children.
    // It can store original material colors, apply new colors, and revert to original or
    // to a globally active view color (e.g., based on land function or energy consumption).
    public class ChildColorToggler : MonoBehaviour
    {
        // Land function ID (e.g., 1: Residential, 2: Commercial). Set by DeepSeekAPI. 0 means unknown/default.
        [HideInInspector]
        public int landFunction = 0;
        // Land energy consumption value (1-100). Set by DeepSeekAPI. 0 means unknown/default.
        [HideInInspector]
        public int landEnergyConsumption = 0;

        // Stores information about a single material instance on a renderer, including its original color.
        private struct OriginalMaterialInstanceInfo
        {
            public Material materialInstance;      // The runtime instance of the material.
            public Color originalColor;            // The original color of the material.
            public bool usesStandardColorProperty; // True if the material uses the common "_Color" property.
            public bool usesBaseColorProperty;     // True if the material uses the common "_BaseColor" property (e.g., URP Lit).
        }

        // Caches original material data. Key: Renderer. Value: List of OriginalMaterialInstanceInfo for its colorizable materials.
        private Dictionary<Renderer, List<OriginalMaterialInstanceInfo>> originalMaterialData =
            new Dictionary<Renderer, List<OriginalMaterialInstanceInfo>>();

        // List of renderers (self and children) that have at least one material whose color can be managed by this script.
        private List<Renderer> managedRenderers = new List<Renderer>();

        // Flag indicating whether the component has been initialized (renderers scanned and original colors stored).
        private bool isInitialized = false;
        // Stores the color that was last applied as part of a global view (e.g., function or energy view).
        // This field is not actively used in the current logic for restoration decision but was present in original.
        private Color currentActiveGlobalViewColorApplied;

        // Initializes or re-initializes the component.
        // It finds all Renderer components in itself and its children,
        // and for each manageable material (those with _Color or _BaseColor properties),
        // it stores the runtime material instance and its original color.
        public void PublicInitializeRenderers()
        {
            managedRenderers.Clear();
            originalMaterialData.Clear();
            isInitialized = false;
            currentActiveGlobalViewColorApplied = Color.clear; // Reset to a non-color state.

            // Get all renderers in this GameObject and its children, including inactive ones.
            Renderer[] renderersToProcess = GetComponentsInChildren<Renderer>(true);

            foreach (Renderer rend in renderersToProcess)
            {
                // Check if the renderer has materials.
                if (rend.materials != null && rend.materials.Length > 0)
                {
                    List<OriginalMaterialInstanceInfo> infosForThisRenderer = new List<OriginalMaterialInstanceInfo>();
                    bool rendererHasAnyManageableMaterial = false;

                    // Iterate through all materials of the current renderer.
                    // Accessing rend.materials[i] gets or creates a runtime instance of the material for this renderer at this slot.
                    for (int i = 0; i < rend.materials.Length; i++)
                    {
                        Material matInstance = rend.materials[i]; // This is the runtime instance.

                        if (matInstance == null) continue; // Skip if material instance is null.

                        OriginalMaterialInstanceInfo matInfo = new OriginalMaterialInstanceInfo
                        {
                            materialInstance = matInstance, // Store the runtime material instance.
                            originalColor = Color.white,    // Default original color.
                            usesStandardColorProperty = false,
                            usesBaseColorProperty = false
                        };

                        bool propertyFoundForThisMaterial = false;
                        // Check for standard "_Color" property.
                        if (matInstance.HasProperty("_Color"))
                        {
                            matInfo.originalColor = matInstance.color;
                            matInfo.usesStandardColorProperty = true;
                            propertyFoundForThisMaterial = true;
                        }
                        // Else, check for "_BaseColor" property (common in URP/HDRP).
                        else if (matInstance.HasProperty("_BaseColor"))
                        {
                            matInfo.originalColor = matInstance.GetColor("_BaseColor");
                            matInfo.usesBaseColorProperty = true;
                            propertyFoundForThisMaterial = true;
                        }

                        // If a colorizable property was found, add its info.
                        if (propertyFoundForThisMaterial)
                        {
                            infosForThisRenderer.Add(matInfo);
                            rendererHasAnyManageableMaterial = true;
                        }
                    }

                    // If this renderer has at least one manageable material, store its data.
                    if (rendererHasAnyManageableMaterial)
                    {
                        originalMaterialData[rend] = infosForThisRenderer;
                        if (!managedRenderers.Contains(rend))
                        {
                            managedRenderers.Add(rend);
                        }
                    }
                }
            }
            isInitialized = true; // Mark as initialized.
        }

        // Sets the color for all manageable materials on all managed renderers.
        // 'isPartOfActiveGlobalView' indicates if this color change is due to a global view (e.g., function/energy).
        public void SetChildrenColor(Color newColor, bool isPartOfActiveGlobalView = false)
        {
            if (!isInitialized) return; // Do nothing if not initialized.

            if (isPartOfActiveGlobalView)
            {
                currentActiveGlobalViewColorApplied = newColor; // Store the color applied due to a global view.
            }

            // Apply the new color to all managed materials.
            foreach (Renderer rend in managedRenderers)
            {
                if (originalMaterialData.TryGetValue(rend, out List<OriginalMaterialInstanceInfo> materialInfoList))
                {
                    foreach (OriginalMaterialInstanceInfo matInstanceInfo in materialInfoList)
                    {
                        if (matInstanceInfo.usesStandardColorProperty)
                        {
                            matInstanceInfo.materialInstance.color = newColor;
                        }
                        else if (matInstanceInfo.usesBaseColorProperty)
                        {
                            matInstanceInfo.materialInstance.SetColor("_BaseColor", newColor);
                        }
                    }
                }
            }
        }

        // Resets the color for all manageable materials on all managed renderers to their stored original colors.
        public void ResetChildrenColorToOriginal()
        {
            if (!isInitialized) return; // Do nothing if not initialized.

            currentActiveGlobalViewColorApplied = Color.clear; // Reset global view color tracking.

            // Restore original colors for all managed materials.
            foreach (Renderer rend in managedRenderers)
            {
                if (originalMaterialData.TryGetValue(rend, out List<OriginalMaterialInstanceInfo> materialInfoList))
                {
                    foreach (OriginalMaterialInstanceInfo matInstanceInfo in materialInfoList)
                    {
                        if (matInstanceInfo.usesStandardColorProperty)
                        {
                            matInstanceInfo.materialInstance.color = matInstanceInfo.originalColor;
                        }
                        else if (matInstanceInfo.usesBaseColorProperty)
                        {
                            matInstanceInfo.materialInstance.SetColor("_BaseColor", matInstanceInfo.originalColor);
                        }
                    }
                }
            }
        }

        // Restores the plot's color to an appropriate "background" state.
        // If a global color view (function or energy) is active via BuildingColorChanger (bcc),
        // it applies the corresponding global view color. Otherwise, it resets to the original color.
        public void RestoreToPreviousState(BuildingColorChanger bcc)
        {
            if (!isInitialized) return; // Do nothing if not initialized.

            if (bcc != null) // Check if BuildingColorChanger reference is provided.
            {
                // If the function-based color view is active.
                if (bcc.IsFunctionColorViewActive)
                {
                    Color targetFunctionColor = Color.clear;
                    bool hasSpecificFunctionColor = false;
                    Color[] funcColors = bcc.GetFunctionColors(); // Get defined function colors.

                    // Check if this plot has a valid function ID and if colors are defined.
                    if (landFunction >= 1 && landFunction <= funcColors.Length) // landFunction is 1-based.
                    {
                        targetFunctionColor = funcColors[landFunction - 1]; // Array is 0-indexed.
                        hasSpecificFunctionColor = true;
                    }

                    if (hasSpecificFunctionColor)
                    {
                        SetChildrenColor(targetFunctionColor, true); // Apply function color, mark as global view.
                    }
                    else // Plot has no specific function, or its function ID is unknown/default (0).
                    {
                        ResetChildrenColorToOriginal(); // Revert to original color.
                    }
                    return; // State handled.
                }

                // If the energy-based color view is active.
                if (bcc.IsEnergyColorViewActive)
                {
                    // Check if this plot has a valid energy consumption value (1-100).
                    if (landEnergyConsumption >= 1 && landEnergyConsumption <= 100)
                    {
                        // Normalize energy consumption (1-100) to a 0-1 range for color interpolation.
                        float t = (landEnergyConsumption - 1) / 99.0f;
                        Color targetEnergyColor = Color.Lerp(bcc.energyEfficientColor, bcc.energyInefficientColor, t);
                        SetChildrenColor(targetEnergyColor, true); // Apply energy color, mark as global view.
                    }
                    else // Plot's energy value is invalid or unknown/default (0).
                    {
                        ResetChildrenColorToOriginal(); // Revert to original color.
                    }
                    return; // State handled.
                }
            }

            // If no BuildingColorChanger is provided, or if no global view is active in it,
            // then reset this plot to its original color.
            ResetChildrenColorToOriginal();
        }
    }
}