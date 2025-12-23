using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Controls the Start Menu UI with VR-friendly button interactions.
/// Compatible with Meta XR SDK.
/// </summary>
public class StartMenuController : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Scene Settings")]
    [Tooltip("Name of the main game scene to load.")]
    public string mainGameSceneName = "MainGameScene";

    [Header("VR Feedback Settings")]
    [Tooltip("Enable haptic feedback on button press (requires OVR controllers).")]
    public bool enableHapticFeedback = true;
    
    [Tooltip("Haptic feedback strength (0-1).")]
    [Range(0f, 1f)]
    public float hapticStrength = 0.5f;
    
    [Tooltip("Haptic feedback duration in seconds.")]
    public float hapticDuration = 0.1f;

    [Header("Loading Settings")]
    [Tooltip("Delay before loading scene (useful for fade effects).")]
    public float loadDelay = 0.5f;

    private void Start()
    {
        Debug.Log("[StartMenuController] Initialized.");

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
        else
        {
            Debug.LogWarning("[StartMenuController] Start Button not assigned!");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonPressed);
        }
        else
        {
            Debug.LogWarning("[StartMenuController] Quit Button not assigned!");
        }
    }

    private void OnStartButtonPressed()
    {
        Debug.Log("[StartMenuController] Start button pressed.");
        TriggerHapticFeedback();
        StartCoroutine(StartGameDelayed());
    }

    private void OnQuitButtonPressed()
    {
        Debug.Log("[StartMenuController] Quit button pressed.");
        TriggerHapticFeedback();
        StartCoroutine(QuitGameDelayed());
    }

    private IEnumerator StartGameDelayed()
    {
        // Disable buttons to prevent double-click
        SetButtonsInteractable(false);
        
        // Optional: Add fade out effect here
        yield return new WaitForSeconds(loadDelay);
        
        // Load the game scene
        Debug.Log("[StartMenuController] Loading scene: " + mainGameSceneName);
        SceneManager.LoadScene(mainGameSceneName);
    }

    private IEnumerator QuitGameDelayed()
    {
        // Disable buttons to prevent double-click
        SetButtonsInteractable(false);
        
        // Optional: Add fade out effect here
        yield return new WaitForSeconds(loadDelay);
        
        // Quit the application
        Debug.Log("[StartMenuController] Quitting application.");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void TriggerHapticFeedback()
    {
        if (!enableHapticFeedback) return;

        // Try to trigger haptics on both controllers
        try
        {
            // Left controller
            OVRInput.SetControllerVibration(hapticStrength, hapticStrength, OVRInput.Controller.LTouch);
            
            // Right controller
            OVRInput.SetControllerVibration(hapticStrength, hapticStrength, OVRInput.Controller.RTouch);
            
            // Schedule stopping the vibration
            StartCoroutine(StopHapticFeedback());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[StartMenuController] Could not trigger haptic feedback: " + e.Message);
        }
    }

    private IEnumerator StopHapticFeedback()
    {
        yield return new WaitForSeconds(hapticDuration);
        
        try
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
        catch
        {
            // Silently fail if OVR is not available
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null)
            startButton.interactable = interactable;
        
        if (quitButton != null)
            quitButton.interactable = interactable;
    }

    /// <summary>
    /// Public method to programmatically start the game
    /// </summary>
    public void StartGame()
    {
        OnStartButtonPressed();
    }

    /// <summary>
    /// Public method to programmatically quit the game
    /// </summary>
    public void QuitGame()
    {
        OnQuitButtonPressed();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonPressed);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonPressed);
    }
}
