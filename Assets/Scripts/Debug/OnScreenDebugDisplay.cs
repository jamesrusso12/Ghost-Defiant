using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Displays debug information on-screen in VR, so you don't need external logging tools
/// </summary>
public class OnScreenDebugDisplay : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Show debug display in VR")]
    public bool showDisplay = true;
    
    [Tooltip("Maximum number of log lines to show")]
    public int maxLines = 20;
    
    [Tooltip("Font size")]
    public int fontSize = 24;
    
    [Header("Position")]
    public Vector3 positionOffset = new Vector3(0, 0.3f, 0.5f);
    public bool followCamera = true;
    
    private TextMeshProUGUI debugText;
    private Canvas debugCanvas;
    private GameObject canvasObject;
    private Queue<string> logMessages = new Queue<string>();
    private StringBuilder logBuilder = new StringBuilder();
    private string logFilePath;
    
    void Start()
    {
        if (!showDisplay) return;
        
        CreateDebugDisplay();
        SetupFileLogging();
        
        // Subscribe to Unity's log messages
        Application.logMessageReceived += HandleLog;
        
        AddLog("===== DEBUG DISPLAY STARTED =====");
        AddLog("OnScreenDebugDisplay initialized");
    }
    
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    void Update()
    {
        if (!showDisplay || canvasObject == null) return;
        
        // Update position to follow camera
        if (followCamera)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                // Try to find VR camera
                mainCam = FindFirstObjectByType<Camera>();
            }
            
            if (mainCam != null)
            {
                canvasObject.transform.position = mainCam.transform.position + mainCam.transform.rotation * positionOffset;
                canvasObject.transform.rotation = mainCam.transform.rotation;
            }
        }
    }
    
    private void CreateDebugDisplay()
    {
        // Create Canvas
        canvasObject = new GameObject("DebugDisplayCanvas");
        canvasObject.transform.SetParent(transform);
        
        debugCanvas = canvasObject.AddComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.WorldSpace;
        debugCanvas.worldCamera = Camera.main;
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Create background panel
        GameObject panel = new GameObject("DebugPanel");
        panel.transform.SetParent(canvasObject.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        panelRect.anchoredPosition = Vector2.zero;
        
        UnityEngine.UI.Image panelImage = panel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f); // Semi-transparent black
        
        // Create text
        GameObject textObject = new GameObject("DebugText");
        textObject.transform.SetParent(panel.transform, false);
        
        debugText = textObject.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = fontSize;
        debugText.color = Color.green;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        debugText.textWrappingMode = TextWrappingModes.Normal;
        
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        // Set initial position
        if (followCamera)
        {
            Camera mainCam = Camera.main ?? FindFirstObjectByType<Camera>();
            if (mainCam != null)
            {
                canvasObject.transform.position = mainCam.transform.position + mainCam.transform.rotation * positionOffset;
                canvasObject.transform.rotation = mainCam.transform.rotation;
            }
        }
        else
        {
            canvasObject.transform.localPosition = positionOffset;
        }
        
        // Scale canvas to be visible in VR
        canvasObject.transform.localScale = Vector3.one * 0.001f;
    }
    
    private void SetupFileLogging()
    {
        // Create log file path
        string logDirectory = Application.persistentDataPath + "/Logs";
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        logFilePath = Path.Combine(logDirectory, $"DebugLog_{timestamp}.txt");
        
        // Write header
        File.WriteAllText(logFilePath, $"===== DEBUG LOG STARTED =====\n");
        File.AppendAllText(logFilePath, $"Time: {System.DateTime.Now}\n");
        File.AppendAllText(logFilePath, $"Unity Version: {Application.unityVersion}\n");
        File.AppendAllText(logFilePath, $"Platform: {Application.platform}\n\n");
    }
    
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Show SimpleRoomOccluder AND LoadoutManager logs
        if (logString.Contains("SimpleRoomOccluder") || logString.Contains("LoadoutManager"))
        {
            string prefix = "";
            Color logColor = Color.green;
            
            switch (type)
            {
                case LogType.Error:
                    prefix = "[ERROR] ";
                    logColor = Color.red;
                    break;
                case LogType.Warning:
                    prefix = "[WARN] ";
                    logColor = Color.yellow;
                    break;
                default:
                    prefix = "[INFO] ";
                    break;
            }
            
            AddLog(prefix + logString, logColor);
            
            // Also write to file
            WriteToFile($"[{type}] {logString}");
            if (!string.IsNullOrEmpty(stackTrace))
            {
                WriteToFile(stackTrace);
            }
        }
    }
    
    public void AddLog(string message, Color? color = null)
    {
        if (!showDisplay) return;
        
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logLine = $"[{timestamp}] {message}";
        
        logMessages.Enqueue(logLine);
        
        // Keep only last maxLines messages
        while (logMessages.Count > maxLines)
        {
            logMessages.Dequeue();
        }
        
        // Update display
        UpdateDisplay(color ?? Color.green);
    }
    
    private void UpdateDisplay(Color textColor)
    {
        if (debugText == null) return;
        
        logBuilder.Clear();
        foreach (string msg in logMessages)
        {
            logBuilder.AppendLine(msg);
        }
        
        debugText.text = logBuilder.ToString();
        debugText.color = textColor;
    }
    
    private void WriteToFile(string message)
    {
        if (string.IsNullOrEmpty(logFilePath)) return;
        
        try
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
            File.AppendAllText(logFilePath, $"[{timestamp}] {message}\n");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
    
    /// <summary>
    /// Public method to add custom log messages
    /// </summary>
    public static void Log(string message, Color? color = null)
    {
        OnScreenDebugDisplay display = FindFirstObjectByType<OnScreenDebugDisplay>();
        if (display != null)
        {
            display.AddLog(message, color);
        }
    }
    
    /// <summary>
    /// Toggle display on/off
    /// </summary>
    public void ToggleDisplay()
    {
        showDisplay = !showDisplay;
        if (canvasObject != null)
        {
            canvasObject.SetActive(showDisplay);
        }
    }
    
    /// <summary>
    /// Get log file path (for retrieving logs later)
    /// </summary>
    public string GetLogFilePath()
    {
        return logFilePath;
    }
}

