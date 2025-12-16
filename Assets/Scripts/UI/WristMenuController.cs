using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WristMenuController : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject menuPanel;
    public Transform wristTransform;
    
    [Header("Canvas Configuration")]
    [Tooltip("Should this menu ignore MRUIConfigurator auto-configuration? Set to true to prevent conflicts.")]
    public bool ignoreAutoConfiguration = true;
    [Tooltip("Canvas layer - use 'UI' layer (layer 5)")]
    public string canvasLayerName = "UI";
    [Tooltip("UI elements layer - use 'UI' for UI elements")]
    public string uiElementsLayerName = "UI";
    
    [Header("Debug")]
    [Tooltip("Enable verbose debug logging (disable for better performance)")]
    public bool enableDebugLogging = false;
    
    [Header("Menu Positioning")]
    [Tooltip("Offset from wrist position")]
    public Vector3 menuOffset = new Vector3(0f, 0.05f, 0.1f);
    [Tooltip("Rotation offset from wrist")]
    public Vector3 menuRotationOffset = new Vector3(45f, 0f, 0f);
    [Tooltip("Scale of the menu")]
    public float menuScale = 0.0005f;
    
    [Header("Toggle Settings")]
    [Tooltip("Button to toggle menu (Button.Two = Y on Left Controller, B on Right Controller)")]
    public OVRInput.Button toggleButton = OVRInput.Button.Two;
    [Tooltip("Controller to check for input")]
    public OVRInput.Controller controller = OVRInput.Controller.LTouch;
    
    [Header("UI Elements")]
    public Button pauseResumeButton;
    public TextMeshProUGUI pauseResumeText;
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public TextMeshProUGUI volumeValueText;
    public TextMeshProUGUI brightnessValueText;
    
    [Header("Hand Interaction Fix")]
    [Tooltip("Gun GameObject to disable when menu is open (so you can use your hand)")]
    public GameObject gunGameObject;
    [Tooltip("If true, gun will be disabled when menu is open")]
    public bool disableGunWhenMenuOpen = true;
    
    private bool isMenuVisible = false;
    private bool isPaused = false;
    private float currentVolume = 1f;
    private float currentBrightness = 1f;
    
    void Start()
    {
        if (enableDebugLogging)
        {
            Debug.Log("[WristMenu] ========== STARTING INITIALIZATION ==========");
            Debug.Log("[WristMenu] GameObject: " + gameObject.name);
            Debug.Log("[WristMenu] Active: " + gameObject.activeSelf);
            Debug.Log("[WristMenu] Menu Panel assigned: " + (menuPanel != null ? menuPanel.name : "NULL"));
        }
        
        // Find wrist transform if not assigned
        if (wristTransform == null)
        {
            FindWristTransform();
        }
        else if (enableDebugLogging)
        {
            Debug.Log("[WristMenu] Wrist transform already assigned: " + wristTransform.name);
        }
        
        // Attach menu directly to left hand anchor (like flashlight)
        if (wristTransform != null)
        {
            AttachToLeftHand();
        }
        else
        {
            Debug.LogError("[WristMenu] ❌ Cannot attach - wrist transform is null!");
        }
        
        // Configure canvas properly for wrist menu (before MRUIConfigurator interferes)
        ConfigureCanvasForWristMenu();
        
        // Initialize menu state
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            isMenuVisible = false;
            if (enableDebugLogging) Debug.Log("[WristMenu] Menu panel found and hidden (press Y to show)");
        }
        else
        {
            Debug.LogError("[WristMenu] Menu panel is not assigned!");
        }
        
        // Setup button listeners
        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.AddListener(TogglePause);
            if (enableDebugLogging) Debug.Log("[WristMenu] Pause button listener added");
        }
        
        // Setup slider listeners
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (enableDebugLogging) Debug.Log("[WristMenu] Volume slider configured");
        }
        
        if (brightnessSlider != null)
        {
            brightnessSlider.value = currentBrightness;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            if (enableDebugLogging) Debug.Log("[WristMenu] Brightness slider configured");
        }
        
        UpdatePauseButtonText();
        
        // Final verification (only if debug logging enabled)
        if (enableDebugLogging)
        {
            Canvas canvasComponent = GetComponent<Canvas>();
            Debug.Log("[WristMenu] ========== INITIALIZATION SUMMARY ==========");
            Debug.Log("[WristMenu] Canvas GameObject: " + gameObject.name);
            Debug.Log("[WristMenu] Canvas Active: " + gameObject.activeSelf);
            Debug.Log("[WristMenu] Canvas Layer: " + gameObject.layer + " (" + LayerMask.LayerToName(gameObject.layer) + ")");
            Debug.Log("[WristMenu] Canvas Render Mode: " + (canvasComponent != null ? canvasComponent.renderMode.ToString() : "NULL"));
            Debug.Log("[WristMenu] Canvas World Camera: " + (canvasComponent != null && canvasComponent.worldCamera != null ? canvasComponent.worldCamera.name : "NULL"));
            Debug.Log("[WristMenu] Menu Panel: " + (menuPanel != null ? menuPanel.name : "NULL"));
            Debug.Log("[WristMenu] Menu Panel Active: " + (menuPanel != null ? menuPanel.activeSelf.ToString() : "N/A"));
            Debug.Log("[WristMenu] Wrist Transform: " + (wristTransform != null ? wristTransform.name : "NULL"));
            Debug.Log("[WristMenu] Parent: " + (transform.parent != null ? transform.parent.name : "NULL"));
            Debug.Log("[WristMenu] Position: " + transform.position);
            Debug.Log("[WristMenu] Scale: " + transform.localScale);
            Debug.Log("[WristMenu] Gun GameObject: " + (gunGameObject != null ? gunGameObject.name : "NULL"));
            Debug.Log("[WristMenu] Disable Gun When Menu Open: " + disableGunWhenMenuOpen);
            Debug.Log("[WristMenu] ========== READY FOR TESTING ==========");
            Debug.Log("[WristMenu] Press Y button (Button.Two) on left controller to toggle menu");
        }
    }
    
    void AttachToLeftHand()
    {
        if (wristTransform == null)
        {
            Debug.LogError("[WristMenu] ❌ Cannot attach - wrist transform is null!");
            return;
        }
        
        if (enableDebugLogging)
        {
            Debug.Log("[WristMenu] ✓ Found left hand anchor: " + wristTransform.name);
            Debug.Log("[WristMenu] Left hand anchor position: " + wristTransform.position);
            Debug.Log("[WristMenu] Left hand anchor active: " + wristTransform.gameObject.activeSelf);
        }
        
        // Parent the entire canvas to the left hand anchor
        transform.SetParent(wristTransform, false);
        
        // Set local position and rotation relative to hand
        transform.localPosition = menuOffset;
        transform.localRotation = Quaternion.Euler(menuRotationOffset);
        transform.localScale = Vector3.one * menuScale;
        
        if (enableDebugLogging)
        {
            Debug.Log($"[WristMenu] ✓ Attached to left hand anchor");
            Debug.Log($"[WristMenu]   Local Position: {transform.localPosition}");
            Debug.Log($"[WristMenu]   Local Rotation: {transform.localEulerAngles}");
            Debug.Log($"[WristMenu]   Local Scale: {transform.localScale}");
            Debug.Log($"[WristMenu]   Parent: {transform.parent.name}");
            Debug.Log($"[WristMenu]   World Position: {transform.position}");
        }
    }
    
    void FindWristTransform()
    {
        if (enableDebugLogging) Debug.Log("[WristMenu] Searching for left hand anchor...");
        
        // Method 1: Try FindFirstObjectByType
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (cameraRig != null)
        {
            wristTransform = cameraRig.leftHandAnchor;
            if (enableDebugLogging) Debug.Log("[WristMenu] Found OVRCameraRig, left hand anchor: " + (wristTransform != null ? wristTransform.name : "NULL"));
            return;
        }
        
        // Method 2: Try GameObject.Find
        GameObject leftHand = GameObject.Find("LeftHandAnchor");
        if (leftHand != null)
        {
            wristTransform = leftHand.transform;
            if (enableDebugLogging) Debug.Log("[WristMenu] Found LeftHandAnchor via GameObject.Find");
            return;
        }
        
        // Method 3: Try alternate name
        leftHand = GameObject.Find("LeftControllerAnchor");
        if (leftHand != null)
        {
            wristTransform = leftHand.transform;
            if (enableDebugLogging) Debug.Log("[WristMenu] Found LeftControllerAnchor via GameObject.Find");
            return;
        }
        
        // Method 4: Search through hierarchy
        OVRCameraRig[] rigs = FindObjectsByType<OVRCameraRig>(FindObjectsSortMode.None);
        if (rigs.Length > 0)
        {
            wristTransform = rigs[0].leftHandAnchor;
            if (enableDebugLogging) Debug.Log("[WristMenu] Found OVRCameraRig via FindObjectsByType, left hand: " + (wristTransform != null ? wristTransform.name : "NULL"));
            return;
        }
        
        Debug.LogError("[WristMenu] Could not find left hand anchor! Make sure OVRCameraRig is in the scene.");
    }
    
    void Update()
    {
        // Try to find wrist transform if still null and attach
        if (wristTransform == null)
        {
            FindWristTransform();
            if (wristTransform != null)
            {
                AttachToLeftHand();
            }
        }
        // Ensure we're still parented to the hand (in case something unparented us)
        else if (transform.parent != wristTransform)
        {
            AttachToLeftHand();
        }
        
        // Check for toggle input - using multiple detection methods for robustness
        bool buttonPressed = false;
        
        // Method 1: Standard OVRInput.GetDown with specific controller
        if (OVRInput.GetDown(toggleButton, controller))
        {
            buttonPressed = true;
            if (enableDebugLogging) Debug.Log("[WristMenu] ✓ Y button detected via OVRInput.GetDown (controller-specific)");
        }
        
        // Method 2: Backup - Check without controller filter (any controller)
        if (!buttonPressed && OVRInput.GetDown(toggleButton))
        {
            buttonPressed = true;
            if (enableDebugLogging) Debug.Log("[WristMenu] ✓ Y button detected via OVRInput.GetDown (any controller)");
        }
        
        // Method 3: Backup - Check raw button state
        if (!buttonPressed && OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            buttonPressed = true;
            if (enableDebugLogging) Debug.Log("[WristMenu] ✓ Y button detected via OVRInput.RawButton.Y");
        }
        
        // Method 4: Additional backup - Check Button.Four (sometimes Y is mapped here)
        if (!buttonPressed && OVRInput.GetDown(OVRInput.Button.Four))
        {
            buttonPressed = true;
            if (enableDebugLogging) Debug.Log("[WristMenu] ✓ Y button detected via Button.Four");
        }
        
        if (buttonPressed)
        {
            if (enableDebugLogging)
            {
                Debug.Log("[WristMenu] ✓✓✓ TOGGLE BUTTON PRESSED ✓✓✓");
                Debug.Log("[WristMenu] Button: " + toggleButton);
                Debug.Log("[WristMenu] Controller: " + controller);
            }
            ToggleMenu();
        }
        
        // Debug controller state every few seconds (for build testing) - only if debug logging enabled
        if (enableDebugLogging && Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
        {
            Debug.Log($"[WristMenu] Status Check - Menu Visible: {isMenuVisible}, Panel Active: {(menuPanel != null ? menuPanel.activeSelf.ToString() : "NULL")}, Parent: {(transform.parent != null ? transform.parent.name : "NULL")}");
            
            // Log controller connection status
            bool leftConnected = OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
            bool rightConnected = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch);
            OVRInput.Controller activeController = OVRInput.GetActiveController();
            Debug.Log($"[WristMenu] Controller Status - Left: {leftConnected}, Right: {rightConnected}, Active: {activeController}");
        }
        
        // No need to update position every frame since we're parented to the hand
        // The menu will automatically follow the hand transform
    }
    
    void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        
        if (enableDebugLogging)
        {
            Debug.Log("[WristMenu] ========== MENU TOGGLE ==========");
            Debug.Log("[WristMenu] Menu state: " + (isMenuVisible ? "VISIBLE" : "HIDDEN"));
            Debug.Log("[WristMenu] Menu Panel GameObject: " + (menuPanel != null ? menuPanel.name : "NULL"));
            Debug.Log("[WristMenu] Canvas GameObject: " + gameObject.name);
            Debug.Log("[WristMenu] Canvas Active: " + gameObject.activeSelf);
            Debug.Log("[WristMenu] Canvas Parent: " + (transform.parent != null ? transform.parent.name : "NULL"));
            Debug.Log("[WristMenu] Canvas Position: " + transform.position);
            Debug.Log("[WristMenu] Canvas Scale: " + transform.localScale);
        }
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuVisible);
            if (enableDebugLogging)
            {
                Debug.Log("[WristMenu] Menu Panel SetActive(" + isMenuVisible + ")");
                Debug.Log("[WristMenu] Menu Panel Active: " + menuPanel.activeSelf);
                Debug.Log("[WristMenu] Menu Panel ActiveInHierarchy: " + menuPanel.activeInHierarchy);
            }
        }
        else
        {
            Debug.LogError("[WristMenu] ❌ Cannot toggle menu - menuPanel is null!");
        }
        
        // Toggle gun visibility to free up hand for interaction
        if (disableGunWhenMenuOpen && gunGameObject != null)
        {
            // When menu opens, hide gun. When menu closes, show gun.
            gunGameObject.SetActive(!isMenuVisible);
            if (enableDebugLogging) Debug.Log("[WristMenu] Gun " + (isMenuVisible ? "DISABLED" : "ENABLED") + " (free hand for menu interaction)");
        }
    }
    
    // Removed UpdateMenuPosition() - no longer needed since menu is parented to hand
    // The menu will automatically follow the hand transform
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        
        UpdatePauseButtonText();
        
        // Notify game manager
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPaused(isPaused);
        }
    }
    
    void UpdatePauseButtonText()
    {
        if (pauseResumeText != null)
        {
            pauseResumeText.text = isPaused ? "Resume" : "Pause";
        }
    }
    
    void OnVolumeChanged(float value)
    {
        currentVolume = value;
        AudioListener.volume = value;
        
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }
    
    void OnBrightnessChanged(float value)
    {
        currentBrightness = value;
        
        if (brightnessValueText != null)
        {
            brightnessValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
        
        // Apply brightness to camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Adjust camera's HDR exposure or post-processing
            // This is a simple approach - you might want to use URP post-processing instead
            mainCamera.allowHDR = true;
        }
        
        // If using URP, you would adjust the exposure in the volume profile
        ApplyBrightnessToVolume(value);
    }
    
    void ApplyBrightnessToVolume(float brightness)
    {
        // This would integrate with Unity's Volume system for URP
        // For now, we'll use a simple approach with RenderSettings
        RenderSettings.ambientIntensity = brightness;
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
    
    public void SetMenuVisible(bool visible)
    {
        isMenuVisible = visible;
        if (menuPanel != null)
        {
            menuPanel.SetActive(visible);
        }
    }
    
    void ConfigureCanvasForWristMenu()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[WristMenu] No Canvas component found on this GameObject!");
            return;
        }
        
        if (enableDebugLogging) Debug.Log("[WristMenu] Configuring Canvas for wrist menu...");
        
        // Force World Space rendering
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Find and assign camera
        if (canvas.worldCamera == null)
        {
            OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (cameraRig != null && cameraRig.centerEyeAnchor != null)
            {
                Camera cam = cameraRig.centerEyeAnchor.GetComponent<Camera>();
                if (cam != null)
                {
                    canvas.worldCamera = cam;
                    if (enableDebugLogging) Debug.Log("[WristMenu] Assigned CenterEyeAnchor camera to Canvas");
                }
            }
            
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
                if (enableDebugLogging) Debug.Log("[WristMenu] Assigned Main camera to Canvas");
            }
        }
        
        // Set proper scale for wrist menu (only if not already parented)
        if (canvas.transform.parent == null)
        {
            if (canvas.transform.localScale.magnitude < 0.0001f || canvas.transform.localScale.magnitude > 1f)
            {
                canvas.transform.localScale = Vector3.one * menuScale;
                if (enableDebugLogging) Debug.Log($"[WristMenu] Set Canvas scale to {menuScale}");
            }
        }
        
        // CRITICAL: Ensure Canvas renders ABOVE passthrough walls
        // For Canvas UI, use high sorting order (render queue doesn't work the same way)
        canvas.sortingOrder = 1000; // Very high value to ensure it renders on top
        canvas.overrideSorting = true; // Ensure this canvas controls its own sorting
        if (enableDebugLogging) Debug.Log($"[WristMenu] ✓ Set Canvas sorting order to {canvas.sortingOrder} (renders on top)");
        
        // Also disable culling on all canvas renderers to prevent disappearing
        CanvasRenderer[] allRenderers = canvas.GetComponentsInChildren<CanvasRenderer>(true);
        int rendererCount = 0;
        foreach (CanvasRenderer renderer in allRenderers)
        {
            if (renderer != null)
            {
                renderer.cullTransparentMesh = false;
                rendererCount++;
            }
        }
        if (enableDebugLogging) Debug.Log($"[WristMenu] ✓ Disabled culling on {rendererCount} canvas renderers");
        
        // Configure layers - FORCE UI layer (layer 5) for everything
        int uiLayer = LayerMask.NameToLayer("UI");
        
        if (uiLayer == -1)
        {
            Debug.LogError("[WristMenu] ❌ UI layer not found! Make sure layer 5 is named 'UI'");
            // Fallback: use layer 5 directly
            uiLayer = 5;
        }
        
        // Set canvas and all children to UI layer (layer 5)
        SetLayerRecursively(gameObject, uiLayer);
        if (enableDebugLogging) Debug.Log($"[WristMenu] ✓ Set Canvas and all children to UI layer (layer {uiLayer})");
        
        // Verify layer was set correctly
        if (gameObject.layer != uiLayer)
        {
            if (enableDebugLogging) Debug.LogWarning($"[WristMenu] ⚠️ Layer mismatch! Expected {uiLayer}, got {gameObject.layer}");
            gameObject.layer = uiLayer; // Force it
        }
        
        if (enableDebugLogging) Debug.Log($"[WristMenu] Canvas layer: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
        
        // Ensure Graphic Raycaster exists
        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            if (enableDebugLogging) Debug.Log("[WristMenu] Added Graphic Raycaster");
        }
        
        // Add CanvasGroup for better visibility control
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            if (enableDebugLogging) Debug.Log("[WristMenu] ✓ Added CanvasGroup component");
        }
        else if (enableDebugLogging)
        {
            Debug.Log("[WristMenu] ✓ CanvasGroup already exists");
        }
        
        // Make sure camera can see the UI layer (layer 5)
        if (canvas.worldCamera != null)
        {
            Camera cam = canvas.worldCamera;
            int uiLayerBit = 1 << uiLayer;
            
            // Add UI layer to camera culling mask
            if ((cam.cullingMask & uiLayerBit) == 0)
            {
                cam.cullingMask |= uiLayerBit;
                if (enableDebugLogging) Debug.Log($"[WristMenu] ✓ Added UI layer ({uiLayer}) to camera culling mask");
            }
            else if (enableDebugLogging)
            {
                Debug.Log($"[WristMenu] ✓ Camera already sees UI layer ({uiLayer})");
            }
            
            if (enableDebugLogging)
            {
                Debug.Log($"[WristMenu] Camera: {cam.name}");
                Debug.Log($"[WristMenu] Camera culling mask: {cam.cullingMask} (binary: {System.Convert.ToString(cam.cullingMask, 2)})");
                Debug.Log($"[WristMenu] UI layer bit ({uiLayer}): " + ((cam.cullingMask & uiLayerBit) != 0 ? "VISIBLE" : "HIDDEN"));
            }
        }
        else if (enableDebugLogging)
        {
            Debug.LogWarning("[WristMenu] ⚠️ Canvas world camera is null!");
        }
        
        if (enableDebugLogging) Debug.Log("[WristMenu] ========== CANVAS CONFIGURATION COMPLETE ==========");
    }
    
    void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}

