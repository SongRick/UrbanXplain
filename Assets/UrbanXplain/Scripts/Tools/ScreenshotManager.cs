using UnityEngine;
using System.IO; // Required for file and directory operations.
using System;    // Required for DateTime functionality.

// Manages taking screenshots in the game or editor.
// Screenshots are saved with a timestamp to a specified folder.
public class ScreenshotManager : MonoBehaviour
{
    [Header("Screenshot Settings")]
    // The key to press to take a screenshot.
    [Tooltip("The key to press to take a screenshot.")]
    public KeyCode screenshotKey = KeyCode.P;

    // Prefix for the screenshot filename.
    [Tooltip("Prefix for the screenshot filename.")]
    public string fileNamePrefix = "Screenshot_";

    // Name of the subfolder where screenshots will be saved.
    // This folder will be created inside the Assets folder (in Editor) or PersistentDataPath (in Build).
    [Tooltip("Name of the subfolder to save screenshots (under Assets or PersistentDataPath).")]
    public string folderName = "Screenshots";

    // Magnification factor for the screenshot resolution.
    // 1 = original resolution, 2 = 2x resolution, etc.
    [Tooltip("Magnification factor for screenshot resolution (1 = original, 2 = 2x, etc.).")]
    [Range(1, 8)]
    public int superSize = 1;

    void Update()
    {
        // Check if the designated screenshot key is pressed.
        if (Input.GetKeyDown(screenshotKey))
        {
            TakeScreenshot();
        }
    }

    // Captures a screenshot and saves it to the specified path.
    public void TakeScreenshot()
    {
        // 1. Determine the directory path for saving screenshots.
        string directoryPath;

#if UNITY_EDITOR
        // In the Unity Editor, save screenshots to a subfolder within the Assets directory
        // for easy access and visibility in the Project window.
        // Application.dataPath points to the Assets folder.
        directoryPath = Path.Combine(Application.dataPath, folderName);
#else
        // In a built game, save screenshots to the persistent data path.
        // Application.persistentDataPath is a writable directory suitable for user-specific data.
        directoryPath = Path.Combine(Application.persistentDataPath, folderName);
#endif

        // 2. Create the directory if it does not already exist.
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log($"Created screenshot folder: {directoryPath}");
        }

        // 3. Generate a unique filename using a timestamp.
        // Format: yyyyMMdd_HHmmssfff (YearMonthDay_HourMinuteSecondMillisecond)
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        string fileName = $"{fileNamePrefix}{timestamp}.png";
        string filePath = Path.Combine(directoryPath, fileName);

        // 4. Capture the screenshot.
        // ScreenCapture.CaptureScreenshot can take an optional 'superSize' argument
        // to increase the resolution of the captured image.
        ScreenCapture.CaptureScreenshot(filePath, superSize);

        // 5. Log a confirmation message with the save path.
        Debug.Log($"Screenshot successful! Saved to: {filePath}");

#if UNITY_EDITOR
        // In the Unity Editor, refresh the AssetDatabase after taking a screenshot.
        // This ensures the new screenshot file immediately appears in the Project window.
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}