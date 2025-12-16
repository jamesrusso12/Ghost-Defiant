using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simplified wrist menu that's guaranteed to work with passthrough
/// Uses a more straightforward approach without complex layer management
/// </summary>
public class SimpleWristMenu : MonoBehaviour
{
    [Header("Menu Setup")]
    public GameObject menuPanel;
    
    [Header("Positioning - SIMPLE")]
    [Tooltip("How far in front of left hand (meters)")]
    public float distanceFromHand = 0.15f;
    [Tooltip("How far up from hand (meters)")]
    public float heightAboveHand = 0.08f;
    [Tooltip("Menu size multiplier (1 = normal, 2 = double size)")]
    public float sizeMultiplier = 1f;
    
    [Header("Controls")]
    public KeyCode testKey = KeyCode.Space; // For testing in editor
    
    [Header("UI Elements")]
    public Button pauseButton;
    public TextMeshProUGUI pauseText;
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public TextMeshProUGUI volumeText;
    public TextMeshProUGUI brightnessText;
    
    private Transform leftHand;
    private bool menuVisible = false;
    private bool isPaused = false;
    private Canvas canvas;
    
    void Start()
    {
        Debug.Log("[SimpleWristMenu] === STARTING ===");
        
        // Get canvas
        canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SimpleWristMenu] NO CANVAS FOUND!");
            return;
        }
        
        // Force World Space
        canvas.renderMode = RenderMode.WorldSpace;
        Debug.Log("[SimpleWristMenu] Canvas set to World Space");
        
        // Set scale
        float scale = 0.001f * sizeMultiplier;
        transform.localScale = Vector3.one * scale;
        Debug.Log($"[SimpleWristMenu] Scale set to {scale}");
        
        // Find left hand
        FindLeftHand();
        
        // Hide menu initially
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            Debug.Log("[SimpleWristMenu] Menu hidden initially");
        }
        else
        {
            Debug.LogError("[SimpleWristMenu] MENU PANEL NOT ASSIGNED!");
        }
        
        // Setup buttons
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
            Debug.Log("[SimpleWristMenu] Pause button setup");
        }
        
        // Setup sliders
        if (volumeSlider != null)
        {
            volumeSlider.value = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChange);
            Debug.Log("[SimpleWristMenu] Volume slider setup");
        }
        
        if (brightnessSlider != null)
        {
            brightnessSlider.value = 1f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChange);
            Debug.Log("[SimpleWristMenu] Brightness slider setup");
        }
        
        Debug.Log("[SimpleWristMenu] === INITIALIZATION COMPLETE ===");
    }
    
    void FindLeftHand()
    {
        // Try multiple methods to find left hand
        OVRCameraRig rig = FindFirstObjectByType<OVRCameraRig>();
        if (rig != null)
        {
            leftHand = rig.leftHandAnchor;
            Debug.Log($"[SimpleWristMenu] Found left hand: {(leftHand != null ? leftHand.name : "NULL")}");
            return;
        }
        
        GameObject leftHandObj = GameObject.Find("LeftHandAnchor");
        if (leftHandObj != null)
        {
            leftHand = leftHandObj.transform;
            Debug.Log("[SimpleWristMenu] Found LeftHandAnchor via Find");
            return;
        }
        
        Debug.LogError("[SimpleWristMenu] COULD NOT FIND LEFT HAND!");
    }
    
    void Update()
    {
        // Keep trying to find hand if null
        if (leftHand == null)
        {
            FindLeftHand();
        }
        
        // Check for toggle input
        // Y button on left controller
        if (OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.LTouch))
        {
            Debug.Log("[SimpleWristMenu] *** Y BUTTON PRESSED ***");
            ToggleMenu();
        }
        
        // Keyboard test (for editor, won't work but good for testing script)
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log("[SimpleWristMenu] *** TEST KEY PRESSED ***");
            ToggleMenu();
        }
        
        // Update menu position if visible
        if (menuVisible && menuPanel != null && leftHand != null)
        {
            UpdateMenuPosition();
        }
    }
    
    void ToggleMenu()
    {
        menuVisible = !menuVisible;
        Debug.Log($"[SimpleWristMenu] === MENU TOGGLED: {(menuVisible ? "VISIBLE" : "HIDDEN")} ===");
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(menuVisible);
            Debug.Log($"[SimpleWristMenu] MenuPanel.SetActive({menuVisible})");
        }
        else
        {
            Debug.LogError("[SimpleWristMenu] Cannot toggle - menuPanel is NULL!");
        }
    }
    
    void UpdateMenuPosition()
    {
        if (leftHand == null || menuPanel == null) return;
        
        // Simple positioning: in front of and above hand
        Vector3 forward = leftHand.forward;
        Vector3 up = leftHand.up;
        
        Vector3 targetPos = leftHand.position + (forward * distanceFromHand) + (up * heightAboveHand);
        menuPanel.transform.position = targetPos;
        
        // Face the player (look at camera)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 lookDir = cam.transform.position - menuPanel.transform.position;
            if (lookDir != Vector3.zero)
            {
                menuPanel.transform.rotation = Quaternion.LookRotation(-lookDir);
            }
        }
    }
    
    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (pauseText != null)
        {
            pauseText.text = isPaused ? "RESUME" : "PAUSE";
        }
        
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPaused(isPaused);
        }
        
        Debug.Log($"[SimpleWristMenu] Game {(isPaused ? "PAUSED" : "RESUMED")}");
    }
    
    void OnVolumeChange(float value)
    {
        AudioListener.volume = value;
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
        Debug.Log($"[SimpleWristMenu] Volume: {value}");
    }
    
    void OnBrightnessChange(float value)
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(0.5f, 1.5f, value);
        if (brightnessText != null)
        {
            brightnessText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
        Debug.Log($"[SimpleWristMenu] Brightness: {value}");
    }
}

