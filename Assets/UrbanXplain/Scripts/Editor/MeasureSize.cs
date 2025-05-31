using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor; // Required for AssetDatabase operations.

// This class measures the dimensions of all prefabs within a specified folder.
// It outputs the results (name, category, has foundation, dimensions) to the console.
public class MeasureSize : MonoBehaviour
{
    // Base path to the folder containing subfolders of prefabs.
    private string baseFolderPath = "Assets/Fantastic City Generator/Buildings/Prefabs/Bases/"; // Renamed from folderPath
    // The last part of the folder path, used to specify a particular subfolder (e.g., "BB", "BC").
    // This should be set in the Inspector or modified as needed.
    public string subfolderName = "BB"; // Renamed from lastPath

    // Called when the script instance is enabled.
    void Start()
    {
        // Combine the base path and subfolder name to get the full path to the prefabs.
        string fullPath = Path.Combine(baseFolderPath, subfolderName); // Renamed for clarity
        // Find all prefabs in the specified folder.
        List<GameObject> prefabs = FindPrefabsInFolder(fullPath);
        // Initialize the output string with header information.
        string outputLog = "PrefabName\tCategory\tHasFoundation\tX_Size\tZ_Size\tY_Size\n"; // Renamed from output
        // Initialize a string to accumulate error messages.
        string errorLog = "";

        // Iterate through all found prefabs.
        foreach (GameObject prefab in prefabs)
        {
            // Process each prefab to get its dimensional information.
            string prefabDataOutput = ProcessPrefab(prefab, ref errorLog); // Renamed from prefabOutput
            // If processing was successful and returned data, append it to the output log.
            if (!string.IsNullOrEmpty(prefabDataOutput))
            {
                outputLog += prefabDataOutput;
            }
        }

        // If any errors occurred during processing, log them.
        if (!string.IsNullOrEmpty(errorLog))
        {
            Debug.LogError("Errors encountered during prefab processing:\n" + errorLog, this);
        }

        // Log the final output containing all prefab dimension data.
        Debug.Log("Prefab Dimension Measurement Results:\n" + outputLog, this);
    }

    // Finds all GameObject prefabs within the specified folder path (including subdirectories).
    List<GameObject> FindPrefabsInFolder(string path)
    {
        List<GameObject> prefabList = new List<GameObject>();
        try
        {
            // Get all file paths ending with ".prefab" in the given directory and its subdirectories.
            string[] fileEntries = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);

            Debug.Log($"Found {fileEntries.Length} .prefab files in folder: {path}", this);

            // Iterate through each found .prefab file path.
            foreach (string entry in fileEntries)
            {
                // Convert the absolute file path to a relative path starting from "Assets/".
                // This is necessary for AssetDatabase.LoadAssetAtPath.
                string relativePath = Path.GetRelativePath(Application.dataPath, entry);
                string assetPath = Path.Combine("Assets", relativePath).Replace("\\", "/"); // Ensure forward slashes.

                // Attempt to load the prefab asset from its path.
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab != null)
                {
                    prefabList.Add(prefab);
                    // Debug.Log($"Successfully loaded prefab: {prefab.name}");
                }
                else
                {
                    // Debug.LogError($"Failed to load prefab asset at path: {assetPath}");
                }
            }
        }
        catch (DirectoryNotFoundException)
        {
            Debug.LogError($"The specified folder was not found: {path}", this);
        }
        return prefabList;
    }

    // Processes a single prefab to calculate its dimensions.
    // It considers a potential "foundation" part and a "building" part (or a specific collider).
    // 'errorLog' is passed by reference to accumulate any errors encountered.
    string ProcessPrefab(GameObject prefab, ref string errorLog)
    {
        // Instantiate the prefab to access its hierarchy and components.
        GameObject instance = Instantiate(prefab);
        Vector3 foundationSize = Vector3.zero;
        Vector3 buildingPartSize = Vector3.zero; // Renamed from buildingSize
        bool hasFoundation = false;

        // Try to find a "foundation" part within the instantiated prefab.
        Transform foundationTransform = FindBaseFoundation(instance.transform); // Renamed from foundation
        if (foundationTransform != null)
        {
            hasFoundation = true;
            MeshFilter foundationMeshFilter = foundationTransform.GetComponent<MeshFilter>();
            if (foundationMeshFilter != null && foundationMeshFilter.sharedMesh != null) // Check sharedMesh
            {
                foundationSize = foundationMeshFilter.sharedMesh.bounds.size;
            }
            else
            {
                errorLog += $"{prefab.name}: Foundation part '{foundationTransform.name}' is missing MeshFilter or Mesh; ";
            }
        }

        // Look for a specific collider object named "<PrefabName>-Collider" as a direct child.
        string colliderObjectName = prefab.name + "-Collider"; // Renamed from colliderName
        Transform colliderObjectTransform = null; // Renamed from colliderTransform
        foreach (Transform child in instance.transform)
        {
            if (child.name == colliderObjectName)
            {
                colliderObjectTransform = child;
                break;
            }
        }

        if (colliderObjectTransform != null)
        {
            // If the specific collider object is found, use its mesh dimensions for the building part.
            MeshFilter colliderMeshFilter = colliderObjectTransform.GetComponent<MeshFilter>();
            if (colliderMeshFilter != null && colliderMeshFilter.sharedMesh != null)
            {
                Vector3 colliderSize = colliderMeshFilter.sharedMesh.bounds.size;
                // If the collider's height is significant, use its size.
                if (colliderSize.y >= 5f) // Arbitrary threshold for a "valid" collider height.
                {
                    buildingPartSize = colliderSize;
                }
                else
                {
                    // If collider is too small, fall back to finding the main building part by name.
                    errorLog += $"{prefab.name}: '{colliderObjectName}' found but its Y dimension ({colliderSize.y}) is less than 5. Falling back to named part. ";
                    buildingPartSize = GetNamedBuildingPartSize(instance.transform, prefab.name, ref errorLog);
                }
            }
            else
            {
                errorLog += $"{prefab.name}: Specific collider '{colliderObjectName}' is missing MeshFilter or Mesh. Falling back to named part. ";
                buildingPartSize = GetNamedBuildingPartSize(instance.transform, prefab.name, ref errorLog);
            }
        }
        else
        {
            // If no specific collider object, find the main building part by recursively searching for a child named like the prefab.
            buildingPartSize = GetNamedBuildingPartSize(instance.transform, prefab.name, ref errorLog);
        }

        // Calculate final dimensions:
        // X and Z dimensions are the maximum of the foundation and building part.
        // Y dimension is the sum of the foundation and building part heights.
        Vector3 finalSize = new Vector3(
            Mathf.Max(foundationSize.x, buildingPartSize.x),
            foundationSize.y + buildingPartSize.y, // Sum heights
            Mathf.Max(foundationSize.z, buildingPartSize.z)
        );

        // Destroy the instantiated prefab instance.
        Destroy(instance);

        // Return a tab-separated string with the prefab's name, category (subfolder),
        // foundation status, and calculated dimensions.
        return $"{prefab.name}\t{subfolderName}\t{(hasFoundation ? "Yes" : "No")}\t{finalSize.x:F2}\t{finalSize.z:F2}\t{finalSize.y:F2}\n";
    }


    // Helper method to get the size of the main building part by finding a named child.
    private Vector3 GetNamedBuildingPartSize(Transform instanceRoot, string prefabName, ref string errorLog)
    {
        Transform buildingPartTransform = FindDeepestChildWithName(instanceRoot, prefabName); // Renamed
        if (buildingPartTransform != null)
        {
            MeshFilter buildingMeshFilter = buildingPartTransform.GetComponent<MeshFilter>();
            if (buildingMeshFilter != null && buildingMeshFilter.sharedMesh != null)
            {
                return buildingMeshFilter.sharedMesh.bounds.size;
            }
            else
            {
                errorLog += $"{prefabName}: Main building part '{buildingPartTransform.name}' is missing MeshFilter or Mesh; ";
            }
        }
        else
        {
            errorLog += $"{prefabName}: Could not find main building part (child named '{prefabName}'); ";
        }
        return Vector3.zero; // Return zero size if not found or no mesh.
    }


    // Finds a child GameObject whose name starts with "Base", assuming this is the foundation.
    Transform FindBaseFoundation(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Case-insensitive check might be more robust: child.name.ToLower().StartsWith("base")
            if (child.name.StartsWith("Base"))
            {
                return child;
            }
        }
        return null; // No foundation part found.
    }

    // Finds the deepest child GameObject in the hierarchy that has the specified target name.
    // This is used to locate the main building mesh if it's nested.
    Transform FindDeepestChildWithName(Transform parent, string targetName)
    {
        Transform deepestMatchingChild = null; // Renamed from deepestChild
        Transform currentSearchRoot = parent; // Renamed from current

        // This loop attempts to traverse down a path of GameObjects named 'targetName'.
        while (true)
        {
            bool foundNextInPath = false; // Renamed from found
            Transform nextInPathCandidate = null; // Store candidate before committing

            foreach (Transform child in currentSearchRoot)
            {
                if (child.name == targetName)
                {
                    nextInPathCandidate = child;
                    foundNextInPath = true;
                    break;
                }
            }

            if (foundNextInPath)
            {
                currentSearchRoot = nextInPathCandidate; // Move deeper
                deepestMatchingChild = currentSearchRoot; // Update the deepest found so far
            }
            else
            {
                break; // No further children with targetName found in this branch.
            }
        }
        // If no child named 'targetName' was found even at the first level,
        // and the parent itself is named 'targetName' (e.g. prefab root is the building),
        // then return the parent. Otherwise, return the deepest child found or null.
        if (deepestMatchingChild == null && parent.name == targetName)
        {
            return parent;
        }
        return deepestMatchingChild; // This could be null if no matching child was found at any depth.
                                     // Original code returned 'parent' if deepestChild was null, which might not be intended
                                     // if the parent itself doesn't match targetName.
                                     // The logic now prefers a named child.
    }
}