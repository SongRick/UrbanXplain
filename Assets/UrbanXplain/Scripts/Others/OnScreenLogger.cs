using UnityEngine;
using System.Collections.Generic;
using UrbanXplain;

public class OnScreenLogger : MonoBehaviour
{
    [Header("Log Settings")]
    [SerializeField] private int maxLogMessages = 20; // Max number of messages to display
    [SerializeField] private bool showLogs = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote; // Key to toggle log visibility (tilde/backtick)

    [Header("Display Settings")]
    [SerializeField] private float xOffset = 10f;
    [SerializeField] private float yOffset = 10f;
    [SerializeField] private float width = 400f;
    [SerializeField] private float heightPerLine = 18f; // Approximate height per log line
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private int fontSize = 14;

    private readonly List<LogMessage> logMessages = new List<LogMessage>();
    private GUIStyle logStyle;
    private Vector2 scrollPosition = Vector2.zero; // For scrollable log area

    private struct LogMessage
    {
        public string message;
        public LogType type;
        public float timestamp; // Optional: for time-based filtering or display

        public LogMessage(string msg, LogType t)
        {
            message = msg;
            type = t;
            timestamp = Time.realtimeSinceStartup;
        }
    }

    void Awake()
    {
        // Ensure only one instance of this logger exists
        if (FindObjectsOfType<OnScreenLogger>().Length > 1)
        {
            Debug.LogWarning("Multiple OnScreenLogger instances found. Destroying this one.");
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); // Keep the logger active across scene loads
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
        InitializeGUIStyle();
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showLogs = !showLogs;
        }
    }

    void InitializeGUIStyle()
    {
        logStyle = new GUIStyle();
        logStyle.fontSize = fontSize;
        logStyle.normal.textColor = defaultColor; // Default, will be overridden per message
        logStyle.wordWrap = true; // Allow text to wrap
        logStyle.padding = new RectOffset(5, 5, 2, 2);
    }


    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Optionally ignore certain types or filter messages here
        // if (type == LogType.Log && !showInfoLogs) return;
        // if (type == LogType.Warning && !showWarningLogs) return;
        // if (type == LogType.Error && !showErrorLogs) return;

        logMessages.Add(new LogMessage(logString, type));
        while (logMessages.Count > maxLogMessages)
        {
            logMessages.RemoveAt(0);
        }

        // If displaying a lot of logs, you might want to scroll to the bottom automatically
        scrollPosition.y = float.MaxValue; // Will be clamped by ScrollView
    }

    void OnGUI()
    {
        if (!showLogs || logMessages.Count == 0)
        {
            return;
        }

        // Ensure style is initialized, important if OnEnable was skipped (e.g. script disabled then enabled)
        if (logStyle == null || logStyle.fontSize != fontSize) // Re-initialize if font size changed in inspector
        {
            InitializeGUIStyle();
        }

        float totalHeight = Mathf.Min(maxLogMessages, logMessages.Count) * heightPerLine + 20f; // +20 for padding/box
        Rect scrollViewRect = new Rect(xOffset, yOffset, width, totalHeight);
        Rect viewRect = new Rect(0, 0, width - 20f, logMessages.Count * heightPerLine); // Inner content rect

        // Draw a background box
        GUI.Box(scrollViewRect, "Console Log");

        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, viewRect, false, true);

        float currentY = 5f; // Start a bit below the top of the scroll view

        for (int i = 0; i < logMessages.Count; i++)
        {
            LogMessage logEntry = logMessages[i];
            switch (logEntry.type)
            {
                case LogType.Warning:
                    logStyle.normal.textColor = warningColor;
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    logStyle.normal.textColor = errorColor;
                    break;
                default: // LogType.Log
                    logStyle.normal.textColor = defaultColor;
                    break;
            }

            // Calculate height for this specific message (can vary with word wrap)
            // float messageHeight = logStyle.CalcHeight(new GUIContent(logEntry.message), width - 20f); // -20f for scrollbar and padding
            // For simplicity, we use a fixed heightPerLine, but CalcHeight is more accurate if lines wrap significantly.

            GUI.Label(new Rect(5, currentY, width - 25f, heightPerLine), logEntry.message, logStyle);
            currentY += heightPerLine;
        }

        GUI.EndScrollView();
    }
}