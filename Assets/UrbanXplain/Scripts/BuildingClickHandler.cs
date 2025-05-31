using UnityEngine;

namespace UrbanXplain
{
    // Handles click events on buildings or land plots within the scene.
    // It manages highlighting the clicked object and displaying an information popup.
    public class BuildingClickHandler : MonoBehaviour
    {
        [Header("Settings")]
        // The color used to highlight the selected object.
        public Color highlightColor = Color.magenta;
        // Defines which layers the raycast should interact with to detect clickable objects.
        public LayerMask clickableLayers;
        // The maximum distance the raycast will travel to detect objects.
        public float maxRaycastDistance = 1000f;

        [Header("References")]
        // Reference to the BuildingColorChanger script, used for managing building color states.
        [Tooltip("Drag the GameObject containing the BuildingColorChanger script here.")]
        public BuildingColorChanger buildingColorChanger;

        [Header("UI References")]
        // Reference to the InfoPopupManager script, responsible for showing and hiding information popups.
        [Tooltip("Drag the GameObject containing the InfoPopupManager script here.")]
        public InfoPopupManager infoPopupManager;

        // Cached reference to the main camera in the scene.
        private Camera mainCamera;
        // Stores the ChildColorToggler component of the currently highlighted land plot.
        private ChildColorToggler currentlyHighlightedToggler = null;
        // The name of the parent GameObject under which all empty land plots are organized.
        // This name must exactly match the name of the parent GameObject in your scene hierarchy.
        private const string EmptyLandsParentName = "EmptyLands000";
        // Reference to the UIControl script, used to check if the UI is currently in an input mode.
        public UIControl uIControl;


        void Start()
        {
            // Cache the main camera for performance.
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("BuildingClickHandler: Main Camera not found! Script will be disabled.");
                enabled = false; // Disable the script if no main camera is found.
                return;
            }

            // Warn if the BuildingColorChanger reference is not set, as color restoration might be affected.
            if (buildingColorChanger == null)
            {
                Debug.LogWarning("BuildingClickHandler: BuildingColorChanger reference not set. Color restore behavior might be limited.");
            }

            // Warn if the InfoPopupManager reference is not set, as popup functionality will be disabled.
            if (infoPopupManager == null)
            {
                Debug.LogWarning("BuildingClickHandler: InfoPopupManager reference not set. Popup functionality will be disabled.");
            }
        }

        void Update()
        {
            // If the UIControl indicates that an input field is active (or similar UI interaction),
            // do not process building/plot clicks to avoid conflicts.
            if (uIControl != null && uIControl.IsInputMode()) // Check with UIControl's state
            {
                return;
            }

            // Check for left mouse button click.
            if (Input.GetMouseButtonDown(0))
            {
                // Prevent click-through if an info panel is active and the click occurs on the panel itself.
                // This is a simplified approach. For more robust UI click handling,
                // consider using EventSystem.current.IsPointerOverGameObject().
                if (infoPopupManager != null && infoPopupManager.infoPanelObject.activeSelf &&
                    RectTransformUtility.RectangleContainsScreenPoint(
                        infoPopupManager.infoPanelObject.GetComponent<RectTransform>(),
                        Input.mousePosition,
                        mainCamera)) // For ScreenSpace-Overlay canvas, mainCamera should be null. For ScreenSpace-Camera/WorldSpace, use the canvas camera.
                {
                    return; // Click was on the UI panel, so do not process it as a world click.
                }

                HandleClick(); // Process the click for world objects.
            }
        }

        // Processes a mouse click to determine if a clickable building or plot was hit.
        void HandleClick()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform a raycast from the mouse position.
            if (Physics.Raycast(ray, out hit, maxRaycastDistance, clickableLayers))
            {
                GameObject hitObject = hit.collider.gameObject;
                Transform currentTransform = hitObject.transform;
                ChildColorToggler clickedPlotToggler = null;
                GameObject landPlotRootGameObjectForID = null; // Stores the root GameObject of the clicked plot for ID retrieval.

                // Traverse up the hierarchy from the hit object to find the land plot's root
                // and its associated ChildColorToggler component.
                // This logic assumes ChildColorToggler is attached to a direct child of EmptyLandsParentName.
                while (currentTransform != null)
                {
                    if (currentTransform.parent != null && currentTransform.parent.name == EmptyLandsParentName)
                    {
                        clickedPlotToggler = currentTransform.GetComponent<ChildColorToggler>();
                        if (clickedPlotToggler != null)
                        {
                            landPlotRootGameObjectForID = currentTransform.gameObject; // This is the identified land plot root.
                            break; // Found the toggler and the root object.
                        }
                    }
                    // If ChildColorToggler could be on deeper children, or the structure is different,
                    // more complex logic would be needed here to correctly identify the "land plot root".

                    if (currentTransform.parent == null) break; // Reached the top of the scene hierarchy.
                    currentTransform = currentTransform.parent;
                }


                if (clickedPlotToggler != null && landPlotRootGameObjectForID != null)
                {
                    // If the clicked plot is the same as the currently highlighted one, deselect it.
                    if (currentlyHighlightedToggler == clickedPlotToggler)
                    {
                        RestorePreviousColor(currentlyHighlightedToggler);
                        currentlyHighlightedToggler = null;
                        if (infoPopupManager != null) infoPopupManager.HidePopup();
                    }
                    else // A new, different plot is clicked.
                    {
                        // Deselect any previously highlighted plot.
                        if (currentlyHighlightedToggler != null)
                        {
                            RestorePreviousColor(currentlyHighlightedToggler);
                        }

                        // Highlight the newly clicked plot.
                        currentlyHighlightedToggler = clickedPlotToggler;
                        currentlyHighlightedToggler.SetChildrenColor(highlightColor, false); // false: not part of a global view.

                        // Attempt to display the information popup for the clicked plot.
                        if (infoPopupManager != null &&
                            buildingColorChanger != null &&
                            buildingColorChanger.deepSeekAPI != null &&
                            buildingColorChanger.deepSeekAPI.buildingSpawnerJson != null)
                        {
                            BuildingSpawnerJson spawner = buildingColorChanger.deepSeekAPI.buildingSpawnerJson;
                            int landId = -1;

                            // Find the land ID by matching the landPlotRootGameObjectForID
                            // with the GameObjects in the spawner's landArray.
                            for (int i = 0; i < spawner.landArray.Length; i++)
                            {
                                if (spawner.landArray[i] == landPlotRootGameObjectForID)
                                {
                                    landId = i + 1; // Land IDs are 1-based, while array indices are 0-based.
                                    break;
                                }
                            }

                            if (landId != -1)
                            {
                                // Successfully found land ID, get its summary and show the popup.
                                string summary = spawner.GetLandSummary(landId);
                                BuildingInfo info = new BuildingInfo(landId.ToString(), summary);
                                infoPopupManager.ShowPopup(info);
                            }
                            else
                            {
                                Debug.LogWarning($"BuildingClickHandler: Could not find LandID for clicked plot '{landPlotRootGameObjectForID.name}'.");
                                if (infoPopupManager != null) infoPopupManager.HidePopup(); // Hide popup if ID couldn't be determined.
                            }
                        }
                        else
                        {
                            // Log a warning if essential references for showing the popup are missing.
                            Debug.LogWarning("BuildingClickHandler: Missing references (InfoPopupManager, BuildingColorChanger, DeepSeekAPI, or BuildingSpawnerJson) to show popup.");
                            if (infoPopupManager != null) infoPopupManager.HidePopup(); // Still try to hide popup for consistent state.
                        }
                    }
                }
                else // The click hit an object on a clickable layer, but it wasn't a recognized land plot part.
                {
                    // Deselect any currently highlighted plot and hide the popup.
                    if (currentlyHighlightedToggler != null)
                    {
                        RestorePreviousColor(currentlyHighlightedToggler);
                        currentlyHighlightedToggler = null;
                    }
                    if (infoPopupManager != null) infoPopupManager.HidePopup();
                }
            }
            else // The raycast did not hit any object on the clickable layers (e.g., clicked empty sky or background).
            {
                // Deselect any currently highlighted plot and hide the popup.
                if (currentlyHighlightedToggler != null)
                {
                    RestorePreviousColor(currentlyHighlightedToggler);
                    currentlyHighlightedToggler = null;
                }
                if (infoPopupManager != null) infoPopupManager.HidePopup();
            }
        }

        // Restores the original color/state of a previously highlighted plot.
        private void RestorePreviousColor(ChildColorToggler togglerToRestore)
        {
            if (togglerToRestore == null) return;
            // Delegate the restoration logic to the ChildColorToggler instance.
            // The BuildingColorChanger might be needed by the toggler to know its original state.
            togglerToRestore.RestoreToPreviousState(buildingColorChanger);
        }
    }
}