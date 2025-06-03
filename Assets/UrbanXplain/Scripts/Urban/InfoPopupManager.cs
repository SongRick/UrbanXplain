using UnityEngine;
using UnityEngine.UI; // Using the legacy UI Text component.

namespace UrbanXplain
{
    // Data structure to hold information about a building or land plot.
    // This is used to populate the information popup.
    public class BuildingInfo
    {
        public string LotID { get; private set; }
        public string Description { get; private set; }

        // Constructor for BuildingInfo.
        public BuildingInfo(string lotID, string description)
        {
            LotID = lotID;
            Description = description;
        }
    }

    // Manages the display and behavior of an information popup UI panel.
    // It requires references to UI elements (Panel, Text fields, Button) to be assigned in the Inspector.
    public class InfoPopupManager : MonoBehaviour
    {
        [Header("UI Elements - Assign in Inspector")]
        // The parent Panel GameObject that contains all elements of the information popup.
        [Tooltip("Drag the parent Panel GameObject that contains all popup elements here.")]
        public GameObject infoPanelObject;

        // The legacy UI Text component used to display the Lot ID.
        [Tooltip("The Text component used to display the lot ID.")]
        public Text lotIDText; // Using legacy Text.

        // The legacy UI Text component used to display the description of the lot/building.
        [Tooltip("The Text component used to display the lot description.")]
        public Text descriptionText; // Using legacy Text.

        // The Button component used to close the information popup.
        [Tooltip("The Button component used to close the popup.")]
        public Button closeButton;

        void Start()
        {
            // Check if all required UI elements have been assigned in the Inspector.
            if (infoPanelObject == null || lotIDText == null || descriptionText == null || closeButton == null)
            {
                Debug.LogError("InfoPopupManager: One or more UI elements have not been assigned in the Inspector! Popup functionality will be affected.");
                // Attempt to hide the panel even if some elements are missing, to avoid partial display.
                if (infoPanelObject != null) infoPanelObject.SetActive(false);
                return;
            }

            // Initially hide the information panel when the scene starts.
            infoPanelObject.SetActive(false);
            // Add a listener to the close button's onClick event to call the HidePopup method.
            closeButton.onClick.AddListener(HidePopup);
        }

        // Displays the information popup with the provided BuildingInfo data.
        public void ShowPopup(BuildingInfo info)
        {
            if (info == null)
            {
                Debug.LogError("InfoPopupManager: BuildingInfo provided to ShowPopup is null!");
                HidePopup(); // Ensure the popup is hidden if no valid information is provided.
                return;
            }

            // Double-check UI elements in case they were unassigned after Start.
            if (infoPanelObject == null || lotIDText == null || descriptionText == null)
            {
                Debug.LogError("InfoPopupManager: UI elements are not properly initialized. Cannot show popup.");
                return;
            }

            // Populate the Text fields with information from the BuildingInfo object.
            lotIDText.text = "Lot ID: " + info.LotID; // A customizable prefix for the Lot ID.
            descriptionText.text = info.Description;

            // Make the information panel visible.
            infoPanelObject.SetActive(true);
        }

        // Hides the information popup panel.
        public void HidePopup()
        {
            if (infoPanelObject != null)
            {
                infoPanelObject.SetActive(false);
            }
        }

        // Optional: A context menu item to test showing the popup from the editor.
        // Right-click on the script component in the Inspector and select "Test Show Info Popup".
        [ContextMenu("Test Show Info Popup")]
        void TestShowPopup() // Renamed for clarity from original "TestShow"
        {
            BuildingInfo testData = new BuildingInfo(
                "Test-LOT-001",
                "This is a test description for the info popup functionality. Ensure all UI elements are correctly connected and display properly."
            );
            ShowPopup(testData);
        }
    }
}