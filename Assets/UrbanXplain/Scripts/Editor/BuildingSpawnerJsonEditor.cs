using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
// System.Collections is implicitly included by Generic, but can be kept for clarity if needed.

namespace UrbanXplain
{
    // Custom editor for the BuildingSpawnerJson script.
    // This extends the default Inspector panel with custom buttons to automate
    // loading building prefabs from specified project paths and
    // populating the land array from children of a designated parent GameObject.
    [CustomEditor(typeof(BuildingSpawnerJson))]
    public class BuildingSpawnerJsonEditor : Editor
    {
        // Array of predefined paths within the Assets folder where building prefabs are located.
        private string[] prefabPaths = {
            "Assets/Fantastic City Generator/Buildings/Prefabs/BB",
            "Assets/Fantastic City Generator/Buildings/Prefabs/BC",
            "Assets/Fantastic City Generator/Buildings/Prefabs/BK",
            "Assets/Fantastic City Generator/Buildings/Prefabs/BR",
            "Assets/Fantastic City Generator/Buildings/Prefabs/EC",
            "Assets/Fantastic City Generator/Buildings/Prefabs/SB"
        };

        // Overrides the default Inspector GUI.
        public override void OnInspectorGUI()
        {
            // Draw the default Inspector elements for the BuildingSpawnerJson script.
            base.OnInspectorGUI();

            // Get a reference to the BuildingSpawnerJson instance being inspected.
            BuildingSpawnerJson script = (BuildingSpawnerJson)target;

            // Add a button to the Inspector to load building prefabs from the predefined paths.
            if (GUILayout.Button("Load Prefabs from Paths")) // Button text changed for clarity
            {
                // Clear the existing buildings array.
                script.buildings = new GameObject[0];
                // Create a list to temporarily store all found building prefabs.
                List<GameObject> allFoundBuildings = new List<GameObject>(); // Renamed for clarity

                // Iterate through each specified prefab path.
                foreach (string path in prefabPaths)
                {
                    // Find all asset GUIDs for prefabs within the current path.
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
                    // Iterate through all found GUIDs.
                    foreach (string guid in guids)
                    {
                        // Convert the GUID to an asset path.
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        // Load the GameObject prefab from the asset path.
                        GameObject buildingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath); // Renamed for clarity
                        // If the prefab was loaded successfully, add it to the list.
                        if (buildingPrefab != null)
                        {
                            allFoundBuildings.Add(buildingPrefab);
                            // Debug.Log($"Loaded prefab: {assetPath}");
                        }
                    }
                }

                // Convert the list of found prefabs to an array and assign it to the script's buildings field.
                script.buildings = allFoundBuildings.ToArray();
                // Mark the script object as "dirty" so that Unity knows to save these changes.
                EditorUtility.SetDirty(script);
                // Log the number of prefabs successfully loaded.
                Debug.Log($"Successfully loaded {allFoundBuildings.Count} prefabs into 'buildings' array.", script);
            }

            // Add a button to the Inspector to load all direct children of the 'emptyLandsParent'
            // into the 'landArray'.
            if (GUILayout.Button("Load All Empty Lands from Parent")) // Button text changed for clarity
            {
                // Check if the 'emptyLandsParent' GameObject has been assigned in the Inspector.
                if (script.emptyLandsParent == null)
                {
                    Debug.LogError("BuildingSpawnerJsonEditor: 'Empty Lands Parent' is not assigned in the Inspector. Cannot load lands.", script);
                    return;
                }

                // Create a list to temporarily store the land GameObjects.
                List<GameObject> landObjects = new List<GameObject>(); // Renamed for clarity
                // Iterate through all direct child Transforms of the 'emptyLandsParent'.
                foreach (Transform child in script.emptyLandsParent.transform)
                {
                    landObjects.Add(child.gameObject);
                    // Debug.Log($"Registered empty land: Name={child.name}");
                }

                // Convert the list of land GameObjects to an array and assign it to the script's landArray field.
                script.landArray = landObjects.ToArray();

                // Mark the script object as "dirty" to save changes.
                EditorUtility.SetDirty(script);
                // Log the number of land GameObjects successfully loaded.
                Debug.Log($"Successfully loaded {script.landArray.Length} empty lands into 'landArray'.", script);
            }
        }
    }
}