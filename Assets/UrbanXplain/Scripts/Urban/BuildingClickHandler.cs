// --- START OF FILE: BuildingClickHandler.cs ---

using UnityEngine;
using UnityEngine.EventSystems; // 引入 EventSystem 以便检查 UI 交互

namespace UrbanXplain
{
    /// <summary>
    /// Manages user clicks on buildings within the scene. It handles highlighting
    /// the clicked building/plot and displaying its detailed information via the BuildingInfoPanel.
    /// This script differentiates between gameplay mode (clicking disabled) and UI input mode.
    /// </summary>
    public class BuildingClickHandler : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("The color to apply to a building/plot when it is selected.")]
        public Color highlightColor = Color.magenta;
        [Tooltip("The layers that this script should consider clickable.")]
        public LayerMask clickableLayers;
        [Tooltip("The maximum distance for the raycast to detect clickable objects.")]
        public float maxRaycastDistance = 1000f;

        [Header("Required References")]
        [Tooltip("Reference to the BuildingColorChanger script to handle color states.")]
        public BuildingColorChanger buildingColorChanger;
        [Tooltip("Reference to the new BuildingInfoPanel script for displaying detailed information.")]
        public BuildingInfoPanel buildingInfoPanel; // 新的面板管理器引用
        [Tooltip("Reference to the UIControl script to check for input mode.")]
        public UIControl uIControl; // 确保这个引用在 Inspector 中设置了

        private Camera mainCamera;
        private ChildColorToggler currentlyHighlightedToggler = null;
        private const string EmptyLandsParentName = "EmptyLands000";

        void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("BuildingClickHandler: Main Camera not found! The script will be disabled.", this);
                enabled = false;
                return;
            }

            // Check for required references and log warnings if they are missing.
            if (buildingColorChanger == null)
            {
                Debug.LogWarning("BuildingClickHandler: BuildingColorChanger reference is not set. Color restoration might not work correctly.", this);
            }
            if (buildingInfoPanel == null)
            {
                Debug.LogWarning("BuildingClickHandler: BuildingInfoPanel reference is not set. No information popup will be shown.", this);
            }
            if (uIControl == null)
            {
                Debug.LogWarning("BuildingClickHandler: UIControl reference is not set. Mode-dependent clicking will not function.", this);
            }
        }

        void Update()
        {
            // We only process clicks, not continuous checks.
            if (InputManager.GetGameMouseButtonDown(0)) // Check for left mouse button click
            {
                ProcessClick();
            }
        }

        /// <summary>
        /// Main logic to process a mouse click, determining context (UI vs. World)
        /// and taking appropriate action.
        /// </summary>
        private void ProcessClick()
        {
            // First, check if UIControl is available and if we are in input mode.
            if (uIControl != null && uIControl.IsInputMode())
            {
                // In UI Input Mode (cursor is visible and unlocked)
                // Check if the click was over a UI element.
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    // The click was on a UI element, so we should not interact with the world.
                    // This prevents clicking through UI to select buildings.
                    return;
                }

                // If the click was not on a UI element, proceed to handle world clicks.
                HandleWorldRaycast();
            }
            else if (uIControl != null && !uIControl.IsInputMode())
            {
                // In Gameplay Mode (cursor is locked)
                // Clicks should not select buildings. Deselect any currently highlighted building.
                DeselectCurrentBuilding();
            }
            else
            {
                // Fallback or error case: UIControl is not assigned.
                // For safety, we can assume world clicks are always processed if UIControl is missing.
                HandleWorldRaycast();
            }
        }

        /// <summary>
        /// Performs a raycast from the camera to the mouse position to detect and handle
        /// clicks on objects in the 3D world.
        /// </summary>
        private void HandleWorldRaycast()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, clickableLayers))
            {
                // Raycast hit a clickable object.
                GameObject hitObject = hit.collider.gameObject;
                ChildColorToggler clickedPlotToggler = FindPlotToggler(hitObject);

                if (clickedPlotToggler != null)
                {
                    // A valid building plot was clicked.
                    if (currentlyHighlightedToggler == clickedPlotToggler)
                    {
                        // Clicked the same plot again, so deselect it.
                        DeselectCurrentBuilding();
                    }
                    else
                    {
                        // A new plot was clicked. Deselect the old one and select the new one.
                        DeselectCurrentBuilding();
                        SelectBuilding(clickedPlotToggler);
                    }
                }
                else
                {
                    // Clicked on something on a clickable layer, but it's not a recognized plot.
                    // Deselect any currently selected building.
                    DeselectCurrentBuilding();
                }
            }
            else
            {
                // Raycast hit nothing, so deselect any currently selected building.
                DeselectCurrentBuilding();
            }
        }

        /// <summary>
        /// Traverses up the hierarchy from a hit object to find the root plot's ChildColorToggler.
        /// </summary>
        /// <param name="hitObject">The initial object hit by the raycast.</param>
        /// <returns>The ChildColorToggler if found, otherwise null.</returns>
        private ChildColorToggler FindPlotToggler(GameObject hitObject)
        {
            Transform currentTransform = hitObject.transform;
            while (currentTransform != null)
            {
                if (currentTransform.parent != null && currentTransform.parent.name == EmptyLandsParentName)
                {
                    // This is the root object for a land plot.
                    return currentTransform.GetComponent<ChildColorToggler>();
                }
                currentTransform = currentTransform.parent;
            }
            return null;
        }

        /// <summary>
        /// Selects a new building plot, highlights it, and shows its information panel.
        /// </summary>
        /// <param name="plotToggler">The toggler of the plot to be selected.</param>
        private void SelectBuilding(ChildColorToggler plotToggler)
        {
            currentlyHighlightedToggler = plotToggler;
            currentlyHighlightedToggler.SetChildrenColor(highlightColor, false); // Highlight the plot.
            ShowBuildingDetails(plotToggler.gameObject); // Show its info.
        }

        /// <summary>
        /// Deselects the currently highlighted building plot, restores its color, and hides the info panel.
        /// </summary>
        private void DeselectCurrentBuilding()
        {
            if (currentlyHighlightedToggler != null)
            {
                RestorePreviousColor(currentlyHighlightedToggler);
                currentlyHighlightedToggler = null;
            }
            if (buildingInfoPanel != null)
            {
                buildingInfoPanel.Hide();
            }
        }

        /// <summary>
        /// Retrieves detailed data for a given plot and displays it in the BuildingInfoPanel.
        /// </summary>
        /// <param name="landPlotRoot">The root GameObject of the land plot.</param>
        private void ShowBuildingDetails(GameObject landPlotRoot)
        {
            // Ensure all necessary references are available.
            if (buildingInfoPanel == null || buildingColorChanger?.deepSeekAPI?.buildingSpawnerJson == null)
            {
                Debug.LogWarning("Cannot show building details: required references (BuildingInfoPanel, BuildingSpawnerJson) are missing.");
                return;
            }

            BuildingSpawnerJson spawner = buildingColorChanger.deepSeekAPI.buildingSpawnerJson;
            if (spawner.landArray == null) { Debug.LogError("Spawner landArray is null!"); return; }

            // Find the ID of the clicked plot.
            int landId = -1;
            for (int i = 0; i < spawner.landArray.Length; i++)
            {
                if (spawner.landArray[i] == landPlotRoot)
                {
                    landId = i + 1; // Land IDs are 1-based.
                    break;
                }
            }

            if (landId != -1)
            {
                // Retrieve the full data for this plot from the spawner.
                var landDataNullable = spawner.GetLandData(landId);
                if (landDataNullable.HasValue)
                {
                    var landData = landDataNullable.Value;

                    // Create a data object specifically for display purposes.
                    BuildingDisplayData displayData = new BuildingDisplayData
                    {
                        LotID = landData.ID.ToString(),
                        // Map internal codes to human-readable strings for the UI.
                        Function = MapFunctionCodeToString(landData.Function),
                        FloorType = MapFloorTypeCodeToString(landData.FloorType),
                        Material = MapMaterialCodeToString(landData.Material),
                        Rationale = string.IsNullOrEmpty(landData.Summary) ? "No design rationale is available for this plot." : landData.Summary
                    };

                    // Pass the data to the panel to be shown.
                    buildingInfoPanel.Show(displayData);
                }
                else
                {
                    Debug.LogWarning($"Could not retrieve land data for Land ID {landId}.");
                    buildingInfoPanel.Hide();
                }
            }
            else
            {
                Debug.LogWarning($"Could not find a Land ID for the clicked plot '{landPlotRoot.name}'.");
                buildingInfoPanel.Hide();
            }
        }

        #region Helper Methods for String Mapping
        private string MapFunctionCodeToString(string code)
        {
            switch (code)
            {
                case "1": return "Residential";
                case "2": return "Commercial";
                case "3": return "Public";
                case "4": return "Cultural & Entertainment";
                default: return "Unknown";
            }
        }

        private string MapFloorTypeCodeToString(string code)
        {
            switch (code)
            {
                case "1": return "Low-rise";
                case "2": return "Mid-rise";
                case "3": return "High-rise";
                case "4": return "Super High-rise";
                default: return "";
            }
        }

        private string MapMaterialCodeToString(string code)
        {
            switch (code)
            {
                case "1": return "Glass Curtain Wall";
                case "2": return "Concrete";
                default: return "Unknown";
            }
        }
        #endregion

        /// <summary>
        /// Restores the color of a plot to its state before it was highlighted.
        /// </summary>
        /// <param name="togglerToRestore">The toggler of the plot to be restored.</param>
        private void RestorePreviousColor(ChildColorToggler togglerToRestore)
        {
            if (togglerToRestore == null) return;

            // This relies on the ChildColorToggler's logic to correctly restore
            // either the original color or the global view color (e.g., energy view).
            togglerToRestore.RestoreToPreviousState(buildingColorChanger);
        }
    }
}
// --- END OF FILE: BuildingClickHandler.cs ---