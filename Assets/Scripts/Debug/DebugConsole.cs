using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// On-screen debug console for displaying debug logs in builds.
/// RIGHT CONTROLLER: A button to toggle visibility, B button to clear logs.
/// Keyboard shortcuts (F1/F2) also work as fallback.
/// Note: Uses RIGHT controller only to avoid conflict with wrist menu (LEFT controller Y button).
/// </summary>
public class DebugConsole : MonoBehaviour
{
    [Header("Console Settings")]
    [Tooltip("Maximum number of log lines to display")]
    public int maxLogLines = 50;
    
    [Tooltip("Font size for console text")]
    public int fontSize = 14;
    
    [Tooltip("Show console by default")]
    public bool showOnStart = true;
    
    [Header("Colors")]
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    public Color logColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color errorColor = Color.red;
    
    private struct LogEntry
    {
        public string message;
        public LogType type;
        public float time;
    }
    
    private List<LogEntry> logs = new List<LogEntry>();
    private bool isVisible = true;
    private Vector2 scrollPosition = Vector2.zero;
    private GUIStyle logStyle;
    private GUIStyle boxStyle;
    private Texture2D backgroundTexture;
    
    void Start()
    {
        isVisible = showOnStart;
        Application.logMessageReceived += HandleLog;
        
        // Create background texture
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();
    }
    
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }
    }
    
    void Update()
    {
        // Toggle console with A button (RIGHT controller only - avoid conflict with wrist menu)
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            isVisible = !isVisible;
        }
        
        // Clear logs with B button (RIGHT controller only)
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            logs.Clear();
        }
        
        // Fallback: Keyboard shortcuts still work for testing
        if (Input.GetKeyDown(KeyCode.F1))
        {
            isVisible = !isVisible;
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            logs.Clear();
        }
    }
    
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Only show logs that start with [ to filter out noise
        if (!logString.StartsWith("["))
        {
            return;
        }
        
        LogEntry entry = new LogEntry
        {
            message = logString,
            type = type,
            time = Time.time
        };
        
        logs.Add(entry);
        
        // Limit log count
        if (logs.Count > maxLogLines)
        {
            logs.RemoveAt(0);
        }
        
        // Auto-scroll to bottom
        scrollPosition.y = float.MaxValue;
    }
    
    void OnGUI()
    {
        if (!isVisible) return;
        
        // Initialize styles
        if (logStyle == null)
        {
            logStyle = new GUIStyle(GUI.skin.label);
            logStyle.fontSize = fontSize;
            logStyle.wordWrap = true;
            logStyle.normal.textColor = logColor;
            
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = backgroundTexture;
        }
        
        // Calculate console size (top portion of screen)
        float consoleHeight = Screen.height * 0.4f;
        Rect consoleRect = new Rect(10, 10, Screen.width - 20, consoleHeight);
        
        // Draw background
        GUI.Box(consoleRect, "", boxStyle);
        
        // Draw header
        Rect headerRect = new Rect(consoleRect.x + 10, consoleRect.y + 10, consoleRect.width - 20, 30);
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = fontSize + 2;
        headerStyle.fontStyle = FontStyle.Bold;
        GUI.Label(headerRect, $"Debug Console (RIGHT: A=Toggle, B=Clear) - {logs.Count} logs", headerStyle);
        
        // Draw scrollable log area
        Rect logAreaRect = new Rect(consoleRect.x + 10, consoleRect.y + 45, consoleRect.width - 20, consoleRect.height - 55);
        scrollPosition = GUI.BeginScrollView(logAreaRect, scrollPosition, 
            new Rect(0, 0, logAreaRect.width - 20, logs.Count * (fontSize + 5)));
        
        float yPos = 0;
        foreach (var log in logs)
        {
            // Set color based on log type
            switch (log.type)
            {
                case LogType.Warning:
                    logStyle.normal.textColor = warningColor;
                    break;
                case LogType.Error:
                case LogType.Exception:
                case LogType.Assert:
                    logStyle.normal.textColor = errorColor;
                    break;
                default:
                    logStyle.normal.textColor = logColor;
                    break;
            }
            
            Rect logRect = new Rect(5, yPos, logAreaRect.width - 25, fontSize + 5);
            string timeStamp = $"[{log.time:F2}s] ";
            GUI.Label(logRect, timeStamp + log.message, logStyle);
            yPos += fontSize + 5;
        }
        
        GUI.EndScrollView();
    }
}

