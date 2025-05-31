using UnityEngine;
using System.Collections.Generic;
using System.Text; // Added for StringBuilder

// This script provides utility functions to generate SQL UPDATE or INSERT statements
// based on the Transform properties (name, position, rotation) of child GameObjects.
// It's useful for exporting scene object data to a database.
public class ObjectDataToSQL : MonoBehaviour
{
    // Generates SQL UPDATE statements for all child GameObjects.
    // The generated SQL updates records in an 'emptyland' table,
    // assuming IDs correspond to the order of children.
    // This method can be triggered from the component's context menu in the Inspector.
    [ContextMenu("Generate UPDATE SQL Statements")]
    public void GenerateUpdateSQLStatements()
    {
        List<Transform> allChildren = new List<Transform>();
        GetAllChildrenRecursive(transform, ref allChildren); // Renamed for clarity

        if (allChildren.Count == 0)
        {
            Debug.LogWarning("No child objects found. Cannot generate UPDATE SQL statements.", this);
            return;
        }

        StringBuilder sqlScriptBuilder = new StringBuilder(); // Use StringBuilder for efficient string concatenation.

        for (int i = 0; i < allChildren.Count; i++)
        {
            Transform child = allChildren[i];
            Vector3 position = child.position;    // World position of the child.
            float rotationY = child.eulerAngles.y; // Y-axis rotation (yaw) of the child.
            string name = child.name;             // Name of the child GameObject.

            // Construct the SQL UPDATE statement.
            // Assumes 'ID' in the database is 1-based and corresponds to the child's index + 1.
            // Columns like 'Height' and 'StartPosY' are both set to position.y.
            string sql = $"UPDATE emptyland SET " +
                        $"Name = '{EscapeSqlString(name)}', " + // Escape name string for SQL
                        $"Height = {position.y:F3}, " +
                        $"StartPosX = {position.x:F3}, " +
                        $"StartPosY = {position.y:F3}, " +
                        $"StartPosZ = {position.z:F3}, " +
                        $"RotationY = {rotationY:F3} " + // Format float values
                        $"WHERE ID = {i + 1};\n";

            sqlScriptBuilder.Append(sql);
        }

        // Log the complete SQL script and the number of statements generated.
        Debug.Log(sqlScriptBuilder.ToString());
        Debug.Log($"Generated {allChildren.Count} UPDATE SQL statements.", this);
    }

    // Generates SQL INSERT statements for all child GameObjects.
    // The generated SQL inserts new records into an 'emptyland' table,
    // assuming IDs correspond to the order of children.
    // This method can also be triggered from the component's context menu.
    [ContextMenu("Generate INSERT SQL Statements")]
    public void GenerateInsertSQLStatements()
    {
        List<Transform> allChildren = new List<Transform>();
        GetAllChildrenRecursive(transform, ref allChildren); // Renamed for clarity

        if (allChildren.Count == 0)
        {
            Debug.LogWarning("No child objects found. Cannot generate INSERT SQL statements.", this);
            return;
        }

        StringBuilder sqlScriptBuilder = new StringBuilder(); // Use StringBuilder.

        for (int i = 0; i < allChildren.Count; i++)
        {
            Transform child = allChildren[i];
            Vector3 position = child.position;
            float rotationY = child.eulerAngles.y;
            string name = child.name;

            // Construct the SQL INSERT statement.
            // Assumes 'ID' is 1-based and corresponds to child's index + 1.
            string sql = $"INSERT INTO emptyland (ID, Name, Height, StartPosX, StartPosY, StartPosZ, RotationY) " +
                        $"VALUES ({i + 1}, '{EscapeSqlString(name)}', {position.y:F3}, {position.x:F3}, {position.y:F3}, {position.z:F3}, {rotationY:F3});\n";

            sqlScriptBuilder.Append(sql);
        }

        // Log the complete SQL script and the number of statements generated.
        Debug.Log(sqlScriptBuilder.ToString());
        Debug.Log($"Generated {allChildren.Count} INSERT SQL statements.", this);
    }

    // Recursively gets all child Transforms under a given parent and adds them to the provided list.
    private void GetAllChildrenRecursive(Transform parent, ref List<Transform> childrenList) // Renamed for clarity
    {
        // Iterate through each direct child of the parent.
        foreach (Transform child in parent)
        {
            if (child != null) // Ensure the child is not null before processing.
            {
                childrenList.Add(child); // Add the current child to the list.
                // If the current child has its own children, recursively call this method for them.
                if (child.childCount > 0)
                {
                    GetAllChildrenRecursive(child, ref childrenList);
                }
            }
        }
    }

    // Helper method to escape single quotes in a string for SQL compatibility.
    private string EscapeSqlString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        return value.Replace("'", "''"); // Replace single quote with two single quotes.
    }
}