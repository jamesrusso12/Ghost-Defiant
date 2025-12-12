using UnityEngine;

/// <summary>
/// Shows helpful controller button mappings overlay.
/// Toggle with both grip buttons pressed simultaneously.
/// </summary>
public class ControllerHelpOverlay : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Show help overlay on start")]
    public bool showOnStart = false;
    
    [Tooltip("Auto-hide after this many seconds (0 = manual toggle only)")]
    public float autoHideDelay = 10f;
    
    [Header("Display")]
    public int fontSize = 16;
    public Color backgroundColor = new Color(0, 0, 0, 0.8f);
    public Color textColor = Color.white;
    public Color headerColor = Color.cyan;
    
    private bool isVisible = false;
    private float showTime = 0f;
    private GUIStyle headerStyle;
    private GUIStyle textStyle;
    private GUIStyle boxStyle;
    private Texture2D backgroundTexture;
    
    void Start()
    {
        isVisible = showOnStart;
        if (isVisible)
        {
            showTime = Time.time;
        }
        
        // Create background texture
        backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, backgroundColor);
        backgroundTexture.Apply();
        
        Debug.Log("[ControllerHelp] Press BOTH grip buttons to toggle help overlay");
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
        // Toggle with both grip buttons pressed
        bool leftGrip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
        bool rightGrip = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
        
        if (leftGrip && rightGrip && OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            ToggleHelp();
        }
        
        // Keyboard toggle for testing
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHelp();
        }
        
        // Auto-hide
        if (isVisible && autoHideDelay > 0 && Time.time - showTime > autoHideDelay)
        {
            isVisible = false;
            Debug.Log("[ControllerHelp] Help overlay auto-hidden");
        }
    }
    
    void ToggleHelp()
    {
        isVisible = !isVisible;
        if (isVisible)
        {
            showTime = Time.time;
        }
        Debug.Log($"[ControllerHelp] Help overlay {(isVisible ? "SHOWN" : "HIDDEN")}");
    }
    
    void OnGUI()
    {
        if (!isVisible) return;
        
        // Initialize styles
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = fontSize + 4;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = headerColor;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.padding = new RectOffset(15, 15, 10, 10);
            textStyle.richText = true;
            
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = backgroundTexture;
        }
        
        // Calculate display area (centered)
        float width = Screen.width * 0.8f;
        float height = Screen.height * 0.7f;
        float x = (Screen.width - width) / 2;
        float y = (Screen.height - height) / 2;
        Rect displayRect = new Rect(x, y, width, height);
        
        // Draw background
        GUI.Box(displayRect, "", boxStyle);
        
        // Draw header
        Rect headerRect = new Rect(x, y + 10, width, 40);
        GUI.Label(headerRect, "🎮 CONTROLLER REFERENCE", headerStyle);
        
        // Build help text
        string helpText = BuildHelpText();
        
        // Draw text
        Rect textRect = new Rect(x, y + 60, width, height - 100);
        GUI.Label(textRect, helpText, textStyle);
        
        // Draw footer
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontSize = fontSize - 2;
        Rect footerRect = new Rect(x, y + height - 40, width, 30);
        GUI.Label(footerRect, "<i>Press both GRIP buttons to hide this overlay</i>", textStyle);
        textStyle.alignment = TextAnchor.UpperLeft;
        textStyle.fontSize = fontSize;
    }
    
    string BuildHelpText()
    {
        string text = "";
        
        text += "<size=" + (fontSize + 2) + "><b>LEFT CONTROLLER</b></size>\n";
        text += "• <b>Y Button</b> - Toggle Wrist Menu\n";
        text += "• <b>Grip</b> - Toggle Performance Monitor\n";
        text += "• <b>Thumbstick Press</b> - Reset Performance Stats\n";
        text += "• <b>Trigger</b> - Interact / Select\n\n";
        
        text += "<size=" + (fontSize + 2) + "><b>RIGHT CONTROLLER</b></size>\n";
        text += "• <b>A Button</b> - Toggle Debug Console\n";
        text += "• <b>B Button</b> - Clear Debug Logs\n";
        text += "• <b>Thumbstick UP/DOWN</b> - Adjust UI Distance *\n";
        text += "• <b>Thumbstick Press</b> - Reset UI Distance *\n";
        text += "• <b>Trigger</b> - Shoot / Primary Action\n\n";
        
        text += "<size=" + (fontSize + 2) + "><b>BOTH CONTROLLERS</b></size>\n";
        text += "• <b>Both Grips</b> - Toggle This Help Overlay\n\n";
        
        text += "<size=" + (fontSize - 2) + "><i>* Requires UIDistanceAdjuster component on UI Canvas</i></size>\n";
        
        return text;
    }
}
