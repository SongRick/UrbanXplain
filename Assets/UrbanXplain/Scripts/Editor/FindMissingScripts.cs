using UnityEditor;
using UnityEngine;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindMissingScripts));
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts in Scene"))
        {
            FindInScene();
        }
        if (GUILayout.Button("Find Missing Scripts in Project Prefabs"))
        {
            FindInProject();
        }
    }

    private static void FindInScene()
    {
        GameObject[] goArray = FindObjectsOfType<GameObject>();
        int goCount = 0;
        int componentsCount = 0;
        int missingCount = 0;

        Debug.Log("--- Start Searching Scene for Missing Scripts ---");
        foreach (GameObject g in goArray)
        {
            goCount++;
            Component[] components = g.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                componentsCount++;
                if (components[i] == null)
                {
                    missingCount++;
                    string s = g.name;
                    Transform t = g.transform;
                    while (t.parent != null)
                    {
                        s = t.parent.name + "/" + s;
                        t = t.parent;
                    }
                    Debug.LogWarning($"<color=red>Missing script found on:</color> {s}", g);
                }
            }
        }
        Debug.Log($"--- Scene Search Complete --- \nSearched {goCount} GameObjects, {componentsCount} components, found {missingCount} missing scripts.");
    }

    private static void FindInProject()
    {
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        int missingCount = 0;

        Debug.Log("--- Start Searching Project Prefabs for Missing Scripts ---");
        foreach (string prefabGuid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                Component[] components = child.gameObject.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        missingCount++;
                        Debug.LogWarning($"<color=red>Missing script found in Prefab:</color> {path}", prefab);
                        break; // Found one, no need to check other components on this object
                    }
                }
            }
        }
        Debug.Log($"--- Project Prefab Search Complete --- \nFound {missingCount} GameObjects with missing scripts in prefabs.");
    }
}