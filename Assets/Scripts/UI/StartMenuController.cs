using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Scene Settings")]
    [Tooltip("Name of the main game scene to load.")]
    public string mainGameSceneName = "MainGameScene";

    private void Start()
    {
        Debug.Log("StartMenuController loaded.");

        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        Debug.Log("Starting game: " + mainGameSceneName);
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting application.");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
