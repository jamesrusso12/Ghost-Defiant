using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the wrist-mounted UI that displays game stats (Round, Score, Timer)
/// Toggle with Y button on left controller
/// </summary>
public class WristMenuController : MonoBehaviour
{
    [Header("Menu References")]
    [Tooltip("The main menu panel to show/hide")]
    public GameObject menuPanel;
    
    [Header("Panels")]
    [Tooltip("Game stats panel (Round, Score, Timer)")]
    public GameObject gameStatsPanel;
    [Tooltip("Settings panel (Volume, Brightness, etc.)")]
    public GameObject settingsPanel;
    [Tooltip("End game panel (Game Over screen)")]
    public GameObject endGamePanel;
    
    [Header("Game Stats UI")]
    [Tooltip("Round counter text (e.g., 'Round 3')")]
    public TextMeshProUGUI roundText;
    [Tooltip("Score display text (e.g., 'Score: 45')")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Timer display text (e.g., '23s')")]
    public TextMeshProUGUI timerText;
    [Tooltip("Button to open settings panel")]
    public Button settingsButton;
    
    [Header("End Game UI")]
    [Tooltip("Final score text on end screen")]
    public TextMeshProUGUI finalScoreText;
    [Tooltip("Restart button on end screen")]
    public Button restartButton;
    
    [Header("Settings UI")]
    [Tooltip("Volume slider (0-1)")]
    public Slider volumeSlider;
    [Tooltip("Brightness slider (0-1)")]
    public Slider brightnessSlider;
    [Tooltip("Pause/Resume button")]
    public Button pauseButton;
    [Tooltip("Pause button text")]
    public TextMeshProUGUI pauseButtonText;
    [Tooltip("Back button to return to game stats")]
    public Button backButton;
    
    [Header("Menu Positioning")]
    [Tooltip("Offset from wrist position")]
    public Vector3 menuOffset = new Vector3(0f, 0.05f, 0.1f);
    [Tooltip("Rotation offset from wrist")]
    public Vector3 menuRotationOffset = new Vector3(45f, 0f, 0f);
    [Tooltip("Scale of the menu")]
    public float menuScale = 0.0005f;
    
    [Header("Input")]
    [Tooltip("Button to toggle menu (Y button on left controller)")]
    public OVRInput.Button toggleButton = OVRInput.Button.Two;
    [Tooltip("Which controller to use")]
    public OVRInput.Controller controller = OVRInput.Controller.LTouch;
    
    [Header("Gun Management")]
    [Tooltip("Reference to the gun script on the right hand")]
    public GunScript gunScript;
    [Tooltip("Auto-find gun on start if not assigned")]
    public bool autoFindGun = true;
    
    [Header("Right Hand Objects")]
    [Tooltip("Gun GameObject to hide when menu is open")]
    public GameObject gunGameObject;
    [Tooltip("Flashlight GameObject to hide when menu is open")]
    public GameObject flashlightGameObject;
    [Tooltip("Auto-find right hand objects")]
    public bool autoFindRightHandObjects = true;
    
    [Header("UI Interaction")]
    [Tooltip("Drag the 'UIRayInteractor' (from Meta Building Blocks) here. This enables pointing at UI buttons.")]
    public GameObject uiRayInteractor;
    
    private Transform wristTransform;
    private bool isMenuVisible = false;
    private bool isPaused = false;
    private float currentVolume = 1f;
    private float currentBrightness = 1f;
    
    void Start()
    {
        // Find and attach to left hand
        FindAndAttachToWrist();
        
        // Find gun if not assigned
        if (autoFindGun && gunScript == null)
        {
            FindGun();
        }
        
        // Configure canvas
        ConfigureCanvas();
        
        // Subscribe to game events
        SubscribeToGameEvents();
        
        // Initialize UI
        InitializeUI();
        
        // Setup button listeners
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ShowSettingsPanel);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(ShowGameStatsPanel);
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        // Setup slider listeners
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
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        // Show game stats panel by default
        ShowGameStatsPanel();
        
        // Hide menu by default
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
        
        // Ensure UI Ray Interactor is off at start (will be enabled when menu opens)
        if (uiRayInteractor != null)
        {
            uiRayInteractor.SetActive(false);
        }
    }
    
    void FindAndAttachToWrist()
    {
        // Find OVRCameraRig and get left hand anchor
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (cameraRig != null)
        {
            wristTransform = cameraRig.leftHandAnchor;
        }
        
        if (wristTransform == null)
        {
            Debug.LogError("[WristUI] Could not find LeftHandAnchor!");
            return;
        }
        
        // Attach to wrist
        transform.SetParent(wristTransform, false);
        transform.localPosition = menuOffset;
        transform.localRotation = Quaternion.Euler(menuRotationOffset);
        transform.localScale = Vector3.one * menuScale;
    }
    
    void FindGun()
    {
        // Try to find the gun script in the scene
        gunScript = FindFirstObjectByType<GunScript>();
        
        if (gunScript != null)
        {
            Debug.Log("[WristUI] Gun script found and linked!");
            
            // Auto-find the gun GameObject if not assigned
            if (autoFindRightHandObjects && gunGameObject == null)
            {
                gunGameObject = gunScript.gameObject;
                Debug.Log($"[WristUI] Gun GameObject auto-linked: {gunGameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning("[WristUI] Could not find GunScript in scene. Gun will not be disabled when menu is open.");
        }
        
        // Try to find flashlight if not assigned
        if (autoFindRightHandObjects && flashlightGameObject == null)
        {
            // Look for common flashlight names
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("flashlight") || 
                    obj.name.ToLower().Contains("flash light") ||
                    obj.name.ToLower().Contains("torch"))
                {
                    flashlightGameObject = obj;
                    Debug.Log($"[WristUI] Flashlight GameObject auto-linked: {flashlightGameObject.name}");
                    break;
                }
            }
            
            if (flashlightGameObject == null)
            {
                Debug.LogWarning("[WristUI] Could not auto-find flashlight. Please assign manually if you have one.");
            }
        }
    }
    
    void ConfigureCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) return;
        
        // Set to World Space
        canvas.renderMode = RenderMode.WorldSpace;
        
        // Find and assign camera
        OVRCameraRig cameraRig = FindFirstObjectByType<OVRCameraRig>();
        if (cameraRig != null && cameraRig.centerEyeAnchor != null)
        {
            Camera cam = cameraRig.centerEyeAnchor.GetComponent<Camera>();
            if (cam != null)
            {
                canvas.worldCamera = cam;
            }
        }
        
        // Set high sorting order to render on top
        canvas.sortingOrder = 1000;
        canvas.overrideSorting = true;
        
        // Ensure raycaster exists
        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    void SubscribeToGameEvents()
    {
        GameManager.OnRoundStarted += UpdateRound;
        GameManager.OnScoreChanged += UpdateScore;
        GameManager.OnGameEnded += OnGameEnded;
    }
    
    void InitializeUI()
    {
        if (roundText != null) roundText.text = "Round 0";
        if (scoreText != null) scoreText.text = "Score: 0";
        if (timerText != null) timerText.text = "0s";
        UpdatePauseButtonText();
    }
    
    void Update()
    {
        // Toggle menu with Y button
        if (OVRInput.GetDown(toggleButton, controller))
        {
            ToggleMenu();
        }
    }
    
    // Public method to check if menu is visible (for other scripts like MenuPointerHelper)
    public bool IsMenuVisible()
    {
        return isMenuVisible;
    }
    
    void ToggleMenu()
    {
        isMenuVisible = !isMenuVisible;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuVisible);
            
            // Always show game stats panel when opening menu
            if (isMenuVisible)
            {
                ShowGameStatsPanel();
            }
        }
        
        // Manage gun state - disable when menu is open, enable when closed
        ManageGunState();
    }
    
    void ManageGunState()
    {
        // Disable gun script when menu is visible
        if (gunScript != null)
        {
            gunScript.enabled = !isMenuVisible;
            Debug.Log($"[WristUI] Gun script {(isMenuVisible ? "disabled" : "enabled")}");
        }
        
        // Hide/show gun GameObject
        if (gunGameObject != null)
        {
            gunGameObject.SetActive(!isMenuVisible);
            Debug.Log($"[WristUI] Gun GameObject {(isMenuVisible ? "hidden" : "shown")}: {gunGameObject.name}");
        }
        
        // Hide/show flashlight GameObject
        if (flashlightGameObject != null)
        {
            flashlightGameObject.SetActive(!isMenuVisible);
            Debug.Log($"[WristUI] Flashlight GameObject {(isMenuVisible ? "hidden" : "shown")}: {flashlightGameObject.name}");
        }
        
        // Toggle UI Ray Interactor - enabled when menu visible, disabled when hidden
        if (uiRayInteractor != null)
        {
            uiRayInteractor.SetActive(isMenuVisible);
        }
        
        if (isMenuVisible)
        {
            Debug.Log("[WristUI] ===== MENU OPEN: Gun hidden, UI Ray ENABLED =====");
        }
        else
        {
            Debug.Log("[WristUI] ===== MENU CLOSED: Gun shown, UI Ray DISABLED =====");
        }
    }
    
    // ============================================================
    // PANEL SWITCHING
    // ============================================================
    
    void ShowGameStatsPanel()
    {
        if (gameStatsPanel != null) gameStatsPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
    }
    
    void ShowSettingsPanel()
    {
        if (gameStatsPanel != null) gameStatsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (endGamePanel != null) endGamePanel.SetActive(false);
    }
    
    void ShowEndGamePanel(int finalScore)
    {
        if (gameStatsPanel != null) gameStatsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(true);
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {finalScore}";
        }
        
        // Auto-open menu to show end screen
        if (!isMenuVisible && menuPanel != null)
        {
            isMenuVisible = true;
            menuPanel.SetActive(true);
        }
    }
    
    // ============================================================
    // GAME STATS UPDATES (Called by GameManager via events)
    // ============================================================
    
    void UpdateRound(int round)
    {
        if (roundText != null)
        {
            roundText.text = $"Round {round}";
        }
    }
    
    void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
    
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining) + "s";
        }
    }
    
    void OnGameEnded()
    {
        // Show end game panel with final score
        if (GameManager.instance != null)
        {
            int finalScore = GameManager.instance.GetTotalScore();
            ShowEndGamePanel(finalScore);
        }
        
        Debug.Log("[WristUI] Game ended - showing end screen");
    }
    
    void OnRestartButtonClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.RestartGame();
        }
    }
    
    // ============================================================
    // PAUSE FUNCTIONALITY
    // ============================================================
    
    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPaused(isPaused);
        }
        
        UpdatePauseButtonText();
    }
    
    void UpdatePauseButtonText()
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "Resume" : "Pause";
        }
    }
    
    // ============================================================
    // SETTINGS CONTROLS
    // ============================================================
    
    void OnVolumeChanged(float value)
    {
        currentVolume = value;
        AudioListener.volume = value;
    }
    
    void OnBrightnessChanged(float value)
    {
        currentBrightness = value;
        // Adjust ambient light intensity
        RenderSettings.ambientIntensity = value;
    }
    
    // ============================================================
    // CLEANUP
    // ============================================================
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameManager.OnRoundStarted -= UpdateRound;
        GameManager.OnScoreChanged -= UpdateScore;
        GameManager.OnGameEnded -= OnGameEnded;
    }
}
