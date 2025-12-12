using UnityEngine;
using TMPro;

/// <summary>
/// VR Performance Monitor - displays FPS, frame time, and other performance metrics.
/// Attach to any GameObject in the scene. Toggle with LEFT controller grip button.
/// </summary>
public class VRPerformanceMonitor : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Show performance overlay on start")]
    public bool showOnStart = true;
    
    [Tooltip("Update frequency in seconds")]
    public float updateInterval = 0.5f;
    
    [Tooltip("Font size for display")]
    public int fontSize = 20;
    
    [Header("Performance Thresholds")]
    [Tooltip("FPS below this is considered BAD (red)")]
    public float badFPS = 45f;
    
    [Tooltip("FPS below this is considered WARNING (yellow)")]
    public float warningFPS = 60f;
    
    [Tooltip("Target FPS for VR (usually 72 or 90)")]
    public float targetFPS = 72f;
    
    [Header("Colors")]
    public Color goodColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color badColor = Color.red;
    public Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    private bool isVisible = true;
    private float deltaTime = 0.0f;
    private float fps = 0.0f;
    private float lastUpdateTime = 0.0f;
    private GUIStyle style;
    private GUIStyle boxStyle;
    private Texture2D backgroundTexture;
    
    // Performance tracking
    private int frameCount = 0;
    private float totalTime = 0.0f;
    private float minFPS = 999f;
    private float maxFPS = 0f;
    
    void Start()
    {
        isVisible = showOnStart;
        
        // Create background texture
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();
        
        Debug.Log("[VRPerformanceMonitor] Started. Toggle with LEFT controller grip button.");
    }
    
    void OnDestroy()
    {
        if (backgroundTexture != null)
        {
            Destroy(backgroundTexture);
        }
    }
    
    void Update()
    {
        // Calculate delta time
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        
        // Track stats
        frameCount++;
        totalTime += Time.unscaledDeltaTime;
        
        // Update FPS at interval
        if (Time.time - lastUpdateTime > updateInterval)
        {
            fps = 1.0f / deltaTime;
            
            if (fps < minFPS) minFPS = fps;
            if (fps > maxFPS) maxFPS = fps;
            
            lastUpdateTime = Time.time;
        }
        
        // Toggle with LEFT controller grip button
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))
        {
            isVisible = !isVisible;
            Debug.Log($"[VRPerformanceMonitor] Performance overlay {(isVisible ? "SHOWN" : "HIDDEN")}");
        }
        
        // Keyboard toggle for testing
        if (Input.GetKeyDown(KeyCode.F3))
        {
            isVisible = !isVisible;
        }
        
        // Reset stats with LEFT thumbstick press
        if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
        {
            ResetStats();
        }
    }
    
    void ResetStats()
    {
        minFPS = 999f;
        maxFPS = 0f;
        frameCount = 0;
        totalTime = 0f;
        Debug.Log("[VRPerformanceMonitor] Stats reset");
    }
    
    void OnGUI()
    {
        if (!isVisible) return;
        
        // Initialize styles
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = fontSize;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;
            style.padding = new RectOffset(10, 10, 10, 10);
            
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = backgroundTexture;
        }
        
        // Determine color based on FPS
        Color currentColor;
        if (fps < badFPS)
            currentColor = badColor;
        else if (fps < warningFPS)
            currentColor = warningColor;
        else
            currentColor = goodColor;
        
        style.normal.textColor = currentColor;
        
        // Calculate display area (top-left corner)
        float width = 400f;
        float height = 200f;
        Rect displayRect = new Rect(10, 10, width, height);
        
        // Draw background
        GUI.Box(displayRect, "", boxStyle);
        
        // Build performance text
        float msec = deltaTime * 1000.0f;
        float avgFPS = totalTime > 0 ? frameCount / totalTime : 0;
        
        string text = $"<b>PERFORMANCE MONITOR</b>\n\n";
        text += $"FPS: <b>{fps:0.}</b> ({msec:0.0} ms)\n";
        text += $"Avg: {avgFPS:0.0} | Min: {minFPS:0.0} | Max: {maxFPS:0.0}\n";
        text += $"Target: {targetFPS} FPS\n\n";
        
        // Status indicator
        if (fps < badFPS)
            text += $"<color=#FF0000>⚠️ PERFORMANCE CRITICAL</color>\n";
        else if (fps < warningFPS)
            text += $"<color=#FFFF00>⚠️ PERFORMANCE WARNING</color>\n";
        else if (fps < targetFPS)
            text += $"<color=#FFA500>▲ BELOW TARGET</color>\n";
        else
            text += $"<color=#00FF00>✓ GOOD PERFORMANCE</color>\n";
        
        text += $"\n<size={fontSize - 4}>LEFT Grip: Toggle | LEFT Stick Press: Reset</size>";
        
        // Draw text
        GUI.Label(displayRect, text, style);
        
        // Draw performance graph (simple bar)
        DrawPerformanceBar(displayRect, fps, targetFPS);
    }
    
    void DrawPerformanceBar(Rect displayRect, float currentFPS, float maxFPS)
    {
        // Bar position (at bottom of display rect)
        float barHeight = 10f;
        float barY = displayRect.y + displayRect.height - barHeight - 10f;
        Rect barBackgroundRect = new Rect(displayRect.x + 10, barY, displayRect.width - 20, barHeight);
        
        // Draw background bar
        GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        GUI.DrawTexture(barBackgroundRect, Texture2D.whiteTexture);
        
        // Draw FPS bar
        float fillPercent = Mathf.Clamp01(currentFPS / maxFPS);
        Rect barFillRect = new Rect(barBackgroundRect.x, barBackgroundRect.y, 
            barBackgroundRect.width * fillPercent, barBackgroundRect.height);
        
        // Color based on performance
        if (currentFPS < badFPS)
            GUI.color = badColor;
        else if (currentFPS < warningFPS)
            GUI.color = warningColor;
        else
            GUI.color = goodColor;
            
        GUI.DrawTexture(barFillRect, Texture2D.whiteTexture);
        
        // Reset color
        GUI.color = Color.white;
    }
}
