using UnityEngine;
using UnityEditor;

namespace UrbanXplain
{
    // Provides editor tools accessible via the "CityLLM Tools" menu
    // to automate setup tasks related to land plots and their interaction with other systems.
    public class LandSetupEditor
    {
        // Constant for the name of the parent GameObject containing all land plot GameObjects.
        private const string EmptyLandsParentName = "EmptyLands000";
        // Constant for the name of an optional higher-level parent GameObject (e.g., a scene organizer).
        private const string UrbanXplainParentName = "UrbanXplain"; // Renamed from CityLLMParentName for consistency if it's your project name

        // Adds a ChildColorToggler component to each direct child GameObject of the 'EmptyLandsParentName' GameObject.
        // This is step 1 in a two-step setup process.
        [MenuItem("UrbanXplain Tools/1. Add ChildColorToggler to Land Plots")] // Menu item path
        private static void AddTogglerComponentToLandPlots() // Renamed method
        {
            // Attempt to find the 'UrbanGPTParentName' GameObject first.
            GameObject urbanGPTParent = GameObject.Find(UrbanXplainParentName);
            GameObject emptyLandsParent = null;

            if (urbanGPTParent != null)
            {
                // If 'UrbanGPTParentName' is found, search for 'EmptyLandsParentName' as its child.
                Transform emptyLandsTransform = urbanGPTParent.transform.Find(EmptyLandsParentName);
                if (emptyLandsTransform != null)
                {
                    emptyLandsParent = emptyLandsTransform.gameObject;
                }
                else
                {
                    // Error if 'EmptyLandsParentName' is not found under 'UrbanGPTParentName'.
                    string errorMsg = $"Could not find a child object named '{EmptyLandsParentName}' under the '{UrbanXplainParentName}' GameObject.";
                    Debug.LogError(errorMsg);
                    EditorUtility.DisplayDialog("Error", errorMsg, "OK");
                    return;
                }
            }
            else
            {
                // If 'UrbanGPTParentName' is not found, search for 'EmptyLandsParentName' directly in the scene root.
                emptyLandsParent = GameObject.Find(EmptyLandsParentName);
                if (emptyLandsParent == null)
                {
                    string errorMsg = $"Could not find a GameObject named '{EmptyLandsParentName}' or '{UrbanXplainParentName}/{EmptyLandsParentName}' in the scene.";
                    Debug.LogError(errorMsg);
                    EditorUtility.DisplayDialog("Error", errorMsg, "OK");
                    return;
                }
                else
                {
                    Debug.LogWarning($"'{UrbanXplainParentName}' GameObject not found. Using '{EmptyLandsParentName}' found at the scene root directly.");
                }
            }

            // Final check if 'emptyLandsParent' was successfully located.
            if (emptyLandsParent == null)
            {
                Debug.LogError($"Failed to locate the '{EmptyLandsParentName}' GameObject.", emptyLandsParent); // Context object for log
                return;
            }

            int addedCount = 0;
            int alreadyPresentCount = 0;

            // Iterate through all direct child GameObjects of 'emptyLandsParent'.
            foreach (Transform landPlotTransform in emptyLandsParent.transform)
            {
                GameObject landPlot = landPlotTransform.gameObject;

                // Check if the land plot already has a ChildColorToggler component.
                if (landPlot.GetComponent<ChildColorToggler>() == null)
                {
                    // If not, add a new ChildColorToggler component.
                    Undo.AddComponent<ChildColorToggler>(landPlot); // Use Undo for editor operations.
                    addedCount++;
                    Debug.Log($"Added ChildColorToggler component to '{landPlot.name}'.", landPlot);
                }
                else
                {
                    alreadyPresentCount++;
                    // Debug.Log($"ChildColorToggler component already present on '{landPlot.name}'.");
                }
            }

            // Display a summary message of the operation.
            string message = $"Operation Complete!\nNewly added ChildColorTogglers: {addedCount} plots.\nChildColorTogglers already present: {alreadyPresentCount} plots.";
            Debug.Log(message);
            EditorUtility.DisplayDialog("Operation Complete", message, "OK");
        }

        // Automatically finds all ChildColorToggler components on land plots and assigns them
        // to the 'colorTogglerArray' field of the DeepSeekAPI script instance in the scene.
        // This is step 2 in the setup process.
        [MenuItem("UrbanXplain Tools/2. Auto-Assign Color Togglers to DeepSeekAPI")] // Menu item path
        private static void AutoAssignTogglersToDeepSeekAPI()
        {
            // Find the DeepSeekAPI script instance in the scene.
            DeepSeekAPI deepSeekAPIInstance = Object.FindObjectOfType<DeepSeekAPI>(); // Renamed for clarity
            if (deepSeekAPIInstance == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find an instance of the DeepSeekAPI script in the scene.", "OK");
                return;
            }

            // Locate the 'EmptyLandsParentName' GameObject, similar to the first tool.
            GameObject urbanGPTParent = GameObject.Find(UrbanXplainParentName);
            GameObject emptyLandsParent = null;

            if (urbanGPTParent != null)
            {
                Transform emptyLandsTransform = urbanGPTParent.transform.Find(EmptyLandsParentName);
                if (emptyLandsTransform != null)
                {
                    emptyLandsParent = emptyLandsTransform.gameObject;
                }
            }

            if (emptyLandsParent == null) // If not found under UrbanGPT parent, try a global search.
            {
                emptyLandsParent = GameObject.Find(EmptyLandsParentName);
            }

            if (emptyLandsParent == null)
            {
                EditorUtility.DisplayDialog("Error", $"Could not find the '{EmptyLandsParentName}' GameObject (neither under '{UrbanXplainParentName}' nor globally).", "OK");
                return;
            }

            int childCount = emptyLandsParent.transform.childCount;
            if (childCount == 0)
            {
                EditorUtility.DisplayDialog("Information", $"The '{EmptyLandsParentName}' GameObject has no child objects (land plots).", "OK");
                // Initialize array to empty if no children, to clear any previous assignments.
                Undo.RecordObject(deepSeekAPIInstance, "Clear Color Toggler Array");
                deepSeekAPIInstance.colorTogglerArray = new ChildColorToggler[0];
                EditorUtility.SetDirty(deepSeekAPIInstance);
                return;
            }

            // Record object for Undo functionality before modifying the array.
            Undo.RecordObject(deepSeekAPIInstance, "Assign Color Togglers to DeepSeekAPI");
            deepSeekAPIInstance.colorTogglerArray = new ChildColorToggler[childCount];
            int assignedCount = 0;

            // Iterate through land plots and assign their ChildColorToggler components to the array.
            // This assumes the order of children in the hierarchy corresponds to the desired array order.
            for (int i = 0; i < childCount; i++)
            {
                Transform landPlotTransform = emptyLandsParent.transform.GetChild(i);
                ChildColorToggler toggler = landPlotTransform.GetComponent<ChildColorToggler>();
                if (toggler != null)
                {
                    deepSeekAPIInstance.colorTogglerArray[i] = toggler;
                    assignedCount++;
                }
                else
                {
                    Debug.LogWarning($"Land plot '{landPlotTransform.name}' (index {i}) does not have a ChildColorToggler component. An empty slot will be left in DeepSeekAPI's array. Please run Step 1 first.", landPlotTransform);
                }
            }

            // Mark the DeepSeekAPI instance as "dirty" so Unity saves the changes to its array.
            EditorUtility.SetDirty(deepSeekAPIInstance);

            string message = $"Assigned {assignedCount}/{childCount} Color Togglers to DeepSeekAPI.\nPlease check the 'Color Toggler Array' field on the DeepSeekAPI component.";
            Debug.Log(message);
            EditorUtility.DisplayDialog("Operation Complete", message, "OK");
        }
    }
}