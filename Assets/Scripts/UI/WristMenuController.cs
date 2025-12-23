using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modern Wrist Menu Controller using Meta XR Interaction SDK
/// 
/// REQUIRED CANVAS SETUP (via "Add Ray Interaction to Canvas" or manual):
/// - Canvas (World Space)
/// - PointableCanvas (from Interaction SDK)
/// - RayInteractable (so the ray can hit it)
/// - GraphicRaycaster
/// 
/// The ray interaction is handled automatically by the Interaction SDK.
/// This script only handles menu logic, input, and billboarding.
/// </summary>
public class WristMenuController : MonoBehaviour
{
    // ============================================================
    // INSPECTOR REFERENCES
    // ============================================================
    
    [Header("=== MAIN VISIBILITY ===")]
    [Tooltip("Assign the 'MenuPanel' child object here. We toggle THIS, not the canvas itself.")]
    public GameObject menuRoot;
    
    [Header("=== PANELS ===")]
    [Tooltip("Stats panel showing Score, Time, Round")]
    public GameObject statsPanel;
    
    [Tooltip("Settings panel with Volume, Brightness, Pause")]
    public GameObject settingsPanel;
    
    [Tooltip("End game panel (Game Over screen)")]
    public GameObject endGamePanel;
    
    [Header("=== STATS PANEL UI ===")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI roundText;
    public Button settingsButton;
    
    [Header("=== SETTINGS PANEL UI ===")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Button pauseButton;
    public TextMeshProUGUI pauseButtonText;
    public Button backButton;
    
    [Header("=== END GAME PANEL UI ===")]
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;
    
    [Header("=== BILLBOARDING ===")]
    [Tooltip("If true, menu always faces the player's head")]
    public bool enableBillboarding = true;
    
    [Tooltip("Reference to player camera (auto-finds if null)")]
    public Transform playerCamera;
    
    [Tooltip("Only rotate on Y axis (keeps menu upright)")]
    public bool lockVerticalRotation = true;
    
    [Tooltip("How fast the menu rotates to face you (lower = smoother)")]
    [Range(1f, 20f)]
    public float billboardSpeed = 12f;
    
    [Tooltip("Use local space rotation (better when parented to hand)")]
    public bool useLocalRotation = true;
    
    [Header("=== POSITIONING ===")]
    [Tooltip("If true, menu is parented to hand (simple mode). If false, uses follow script (advanced).")]
    public bool useSimpleParenting = true;
    
    [Header("=== INPUT ===")]
    [Tooltip("Button to toggle menu (Y = Button.Four on LTouch)")]
    public OVRInput.Button toggleButton = OVRInput.Button.Four;
    
    [Tooltip("Controller for input")]
    public OVRInput.Controller inputController = OVRInput.Controller.LTouch;
    
    [Header("=== HAND OBJECTS (Hide when menu open) ===")]
    [Tooltip("Gun GameObject to hide when menu is open")]
    public GameObject gunObject;
    
    [Tooltip("Flashlight GameObject to hide when menu is open")]
    public GameObject flashlightObject;
    
    [Tooltip("Gun script to disable when menu is open")]
    public GunScript gunScript;
    
    [Tooltip("Auto-find hand objects if not assigned")]
    public bool autoFindHandObjects = true;
    
    [Header("=== RAY INTERACTION (Optional) ===")]
    [Tooltip("If using custom UIRayInteractor, assign it here to toggle with menu")]
    public GameObject uiRayInteractor;
    
    // ============================================================
    // PRIVATE STATE
    // ============================================================
    
    private bool isMenuVisible = false;
    private bool isPaused = false;
    private Canvas menuCanvas;
    
    // Settings values
    private float currentVolume = 1f;
    private float currentBrightness = 1f;
    
    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================
    
    private void Awake()
    {
        menuCanvas = GetComponent<Canvas>();
    }
    
    private void Start()
    {
        // Auto-find camera if not assigned (for billboarding if enabled)
        if (playerCamera == null)
        {
            OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (cameraRig != null)
            {
                playerCamera = cameraRig.centerEyeAnchor;
            }
            else
            {
                playerCamera = Camera.main?.transform;
            }
        }
        
        // Auto-find hand objects
        if (autoFindHandObjects)
        {
            FindHandObjects();
        }
        
        // Configure canvas
        ConfigureCanvas();
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Setup slider listeners
        SetupSliderListeners();
        
        // Subscribe to game events
        SubscribeToGameEvents();
        
        // Initialize UI state
        InitializeUI();
        
        // Hide menu by default - toggle the menuRoot, NOT the whole gameObject!
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }
        isMenuVisible = false;
        
        // Ensure custom ray interactor is off at start
        if (uiRayInteractor != null)
        {
            uiRayInteractor.SetActive(false);
        }
        
        // Ensure hand objects are in correct state
        ManageHandObjects();
        
        Debug.Log("[WristMenu] Initialized successfully");
    }
    
    private void Update()
    {
        // Check for toggle input
        if (OVRInput.GetDown(toggleButton, inputController))
        {
            ToggleMenu();
        }
    }
    
    private void LateUpdate()
    {
        // Billboard rotation runs in LateUpdate to override any parent rotations
        // This ensures the menu always faces the player after all other movement is done
        if (enableBillboarding && isMenuVisible && playerCamera != null)
        {
            UpdateBillboard();
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        UnsubscribeFromGameEvents();
    }
    
    // ============================================================
    // INITIALIZATION
    // ============================================================
    
    private void FindHandObjects()
    {
        // Find gun
        if (gunScript == null)
        {
            gunScript = FindFirstObjectByType<GunScript>();
        }
        
        if (gunScript != null && gunObject == null)
        {
            gunObject = gunScript.gameObject;
        }
        
        // Find flashlight
        if (flashlightObject == null)
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                string nameLower = obj.name.ToLower();
                if (nameLower.Contains("flashlight") || nameLower.Contains("torch"))
                {
                    flashlightObject = obj;
                    break;
                }
            }
        }
    }
    
    private void ConfigureCanvas()
    {
        if (menuCanvas == null) return;
        
        menuCanvas.renderMode = RenderMode.WorldSpace;
        
        // Set world camera
        if (playerCamera != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
            {
                menuCanvas.worldCamera = cam;
            }
        }
        
        // High sorting order
        menuCanvas.sortingOrder = 1000;
        
        // Ensure GraphicRaycaster exists
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
        
        // Remove or disable any Image component on the Canvas itself
        // (This prevents the grey background from appearing)
        Image canvasImage = GetComponent<Image>();
        if (canvasImage != null)
        {
            canvasImage.enabled = false;
            Debug.Log("[WristMenu] Disabled Canvas Image component to prevent grey background");
        }
    }
    
    private void SetupButtonListeners()
    {
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ShowSettingsPanel);
        
        if (backButton != null)
            backButton.onClick.AddListener(ShowStatsPanel);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }
    
    private void SetupSliderListeners()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (brightnessSlider != null)
        {
            brightnessSlider.value = currentBrightness;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
    }
    
    private void SubscribeToGameEvents()
    {
        GameManager.OnRoundStarted += SetRound;
        GameManager.OnScoreChanged += SetScore;
        GameManager.OnGameEnded += OnGameEnded;
    }
    
    private void UnsubscribeFromGameEvents()
    {
        GameManager.OnRoundStarted -= SetRound;
        GameManager.OnScoreChanged -= SetScore;
        GameManager.OnGameEnded -= OnGameEnded;
    }
    
    private void InitializeUI()
    {
        SetScore(0);
        SetRound(0);
        SetTime("0:00");
        UpdatePauseButtonText();
        
        // Start on stats panel
        ShowStatsPanel();
    }
    
    // ============================================================
    // MENU TOGGLE & VISIBILITY
    // ============================================================
    
    public void ToggleMenu()
    {
        SetMenuVisible(!isMenuVisible);
    }
    
    public void SetMenuVisible(bool visible)
    {
        isMenuVisible = visible;
        
        // FIX: Toggle the menuRoot (child) instead of gameObject (self)
        // This keeps the script alive to listen for input!
        if (menuRoot != null)
        {
            menuRoot.SetActive(visible);
        }
        else
        {
            // Fallback: If no menuRoot assigned, toggle Canvas component only
            if (menuCanvas != null) 
            {
                menuCanvas.enabled = visible;
            }
            
            // Also toggle Raycaster so invisible buttons can't be clicked
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null) 
            {
                raycaster.enabled = visible;
            }
        }
        
        // Reset to stats panel when opening
        if (visible)
        {
            ShowStatsPanel();
        }
        
        // Manage hand objects
        ManageHandObjects();
        
        Debug.Log($"[WristMenu] Menu {(visible ? "OPENED" : "CLOSED")}");
    }
    
    public bool IsMenuVisible()
    {
        return isMenuVisible;
    }
    
    private void ManageHandObjects()
    {
        // Hide gun when menu is open
        if (gunObject != null)
        {
            gunObject.SetActive(!isMenuVisible);
        }
        
        if (gunScript != null)
        {
            gunScript.enabled = !isMenuVisible;
        }
        
        // Hide flashlight when menu is open
        if (flashlightObject != null)
        {
            flashlightObject.SetActive(!isMenuVisible);
        }
        
        // Toggle custom ray interactor if assigned
        if (uiRayInteractor != null)
        {
            uiRayInteractor.SetActive(isMenuVisible);
            Debug.Log($"[WristMenu] UIRayInteractor {(isMenuVisible ? "ENABLED" : "DISABLED")}");
        }
    }
    
    // ============================================================
    // BILLBOARDING
    // ============================================================
    
    private void UpdateBillboard()
    {
        if (playerCamera == null) return;
        
        // Calculate direction from menu to camera
        Vector3 directionToCamera = playerCamera.position - transform.position;
        
        // Must have some distance to calculate direction
        if (directionToCamera.sqrMagnitude < 0.001f) return;
        
        if (lockVerticalRotation)
        {
            // Only rotate on Y axis (keeps menu upright like a real UI)
            directionToCamera.y = 0;
            
            // If direction becomes zero after removing Y, keep current rotation
            if (directionToCamera.sqrMagnitude < 0.001f) return;
        }
        
        // Calculate target rotation - menu forward should point AWAY from camera
        // This makes the UI face toward the player
        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
        
        if (useLocalRotation && transform.parent != null)
        {
            // Convert world rotation to local rotation relative to parent
            // This is important when menu is parented to the hand
            Quaternion localTarget = Quaternion.Inverse(transform.parent.rotation) * targetRotation;
            
            // Smoothly interpolate to target rotation
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation, 
                localTarget, 
                Time.deltaTime * billboardSpeed
            );
        }
        else
        {
            // Use world rotation (simpler but might not work well when parented)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * billboardSpeed
            );
        }
    }
    
    // ============================================================
    // PANEL SWITCHING
    // ============================================================
    
    public void ShowStatsPanel()
    {
        if (statsPanel != null) statsPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
    }
    
    public void ShowSettingsPanel()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (endGamePanel != null) endGamePanel.SetActive(false);
    }
    
    public void ShowEndGamePanel()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(true);
        
        // Auto-open menu to show end screen
        if (!isMenuVisible)
        {
            SetMenuVisible(true);
        }
    }
    
    // ============================================================
    // PUBLIC STAT SETTERS
    // ============================================================
    
    public void SetScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void SetTime(string timeString)
    {
        if (timeText != null)
        {
            timeText.text = timeString;
        }
    }
    
    public void SetTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        SetTime($"{mins}:{secs:00}");
    }
    
    public void SetRound(int round)
    {
        if (roundText != null)
        {
            roundText.text = $"Round {round}";
        }
    }
    
    // Called by GameManager (keeping old method name for compatibility)
    public void UpdateTimer(float timeRemaining)
    {
        if (timeText != null)
        {
            timeText.text = Mathf.CeilToInt(timeRemaining) + "s";
        }
    }
    
    // ============================================================
    // SETTINGS HANDLERS
    // ============================================================
    
    private void OnVolumeChanged(float value)
    {
        currentVolume = value;
        AudioListener.volume = value;
        Debug.Log($"[WristMenu] Volume set to {value:F2}");
    }
    
    private void OnBrightnessChanged(float value)
    {
        currentBrightness = value;
        RenderSettings.ambientIntensity = value;
        Debug.Log($"[WristMenu] Brightness set to {value:F2}");
    }
    
    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPaused(isPaused);
        }
        
        UpdatePauseButtonText();
        Debug.Log($"[WristMenu] Game {(isPaused ? "PAUSED" : "RESUMED")}");
    }
    
    private void UpdatePauseButtonText()
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "▶ Resume" : "⏸ Pause";
        }
    }
    
    // ============================================================
    // GAME EVENTS
    // ============================================================
    
    private void OnGameEnded()
    {
        if (GameManager.instance != null)
        {
            int finalScore = GameManager.instance.GetTotalScore();
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {finalScore}";
            }
        }
        
        ShowEndGamePanel();
        Debug.Log("[WristMenu] Game ended - showing end screen");
    }
    
    private void OnRestartClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
        
        SetMenuVisible(false);
    }
}
