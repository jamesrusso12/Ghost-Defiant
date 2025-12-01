using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI Elements")]
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject endScreen;
    public TextMeshProUGUI finalScoreText;

    [Header("References")]
    public GhostSpawner ghostSpawner;

    [Header("Game Settings")]
    [SerializeField] private int[] roundTimes = { 60, 55, 50, 45, 40 };
    [SerializeField] private int[] ghostsPerRound = { 15, 20, 25, 30, 35 };
    [SerializeField] private float[] spawnRates = { 1f, 0.8f, 0.6f, 0.4f, 0.3f };

    private int currentRound = 0;
    private int totalGhostsKilled = 0;
    private int ghostsToKill = 0;
    private int ghostsKilledThisRound = 0;

    private float timeRemaining;
    private bool roundActive = false;
    private bool isTransitioning = false;

    // Events for better decoupling
    public static event Action<int> OnRoundStarted;
    public static event Action<int> OnRoundEnded;
    public static event Action<int> OnScoreChanged;
    public static event Action OnGameEnded;

    private MRUIConfigurator uiConfigurator;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        // Configure UI for Mixed Reality
        ConfigureUIForMR();
        
        StartNextRound();
    }
    
    private void ConfigureUIForMR()
    {
        // Find or create MRUI Configurator
        uiConfigurator = FindFirstObjectByType<MRUIConfigurator>();
        if (uiConfigurator == null)
        {
            GameObject configuratorObj = new GameObject("MRUI Configurator");
            uiConfigurator = configuratorObj.AddComponent<MRUIConfigurator>();
        }

        // Configure all canvases
        if (uiConfigurator != null)
        {
            uiConfigurator.ConfigureAllCanvases();
        }

        // Ensure UI elements are properly configured
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            // Make sure UI elements are visible and properly scaled for MR
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                // Ensure world space UI is properly scaled
                if (canvas.transform.localScale.magnitude < 0.0001f)
                {
                    canvas.transform.localScale = Vector3.one * 0.001f;
                }
            }
        }
    }

    private void Update()
    {
        if (roundActive)
        {
            timeRemaining -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(timeRemaining) + "s";

            if (timeRemaining <= 0)
            {
                EndRound();
            }
        }
    }

    void StartNextRound()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        currentRound++;

        if (currentRound > roundTimes.Length)
        {
            EndGame();
            return;
        }

        BeginRound(currentRound);
    }

    void BeginRound(int round)
    {
        roundActive = true;
        isTransitioning = false;

        ghostsKilledThisRound = 0;
        ghostsToKill = ghostsPerRound[round - 1];
        timeRemaining = roundTimes[round - 1];

        roundText.text = $"Round {round}";
        scoreText.text = $"Score: {totalGhostsKilled}";

        // Notify other systems about round start
        OnRoundStarted?.Invoke(round);

        ghostSpawner.BeginRound(round);
    }

    public void GhostKilled()
    {
        ghostsKilledThisRound++;
        totalGhostsKilled++;

        scoreText.text = $"Score: {totalGhostsKilled}";
        OnScoreChanged?.Invoke(totalGhostsKilled);

        if (ghostsKilledThisRound >= ghostsToKill)
        {
            EndRound(); // ends if all ghosts are killed
        }
    }

    void EndRound()
    {
        if (!roundActive) return;

        roundActive = false;
        ghostSpawner.StopSpawning();

        OnRoundEnded?.Invoke(currentRound);
        ClearAllGhosts();

        StartCoroutine(RoundTransition());
    }

    void ClearAllGhosts()
    {
        Ghost[] allGhosts = FindObjectsByType<Ghost>(FindObjectsSortMode.None);
        foreach (Ghost ghost in allGhosts)
        {
            Destroy(ghost.gameObject);
        }
    }


    IEnumerator RoundTransition()
    {
        RectTransform rt = roundText.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        roundText.text = $"Round {currentRound} Complete!";

        yield return new WaitForSeconds(3f);

        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(30f, -30f);

        StartNextRound();
    }

    void EndGame()
    {
        roundActive = false;
        ghostSpawner.StopSpawning();

        OnGameEnded?.Invoke();

        roundText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);

        endScreen.SetActive(true);
        finalScoreText.text = $"Final Score: {totalGhostsKilled}";
    }

    public void AddScore(int points)
    {
        totalGhostsKilled += points;
        scoreText.text = $"Score: {totalGhostsKilled}";
    }

    public void RestartGame()
    {
        int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (SceneTransitionManager.singleton != null)
        {
            SceneTransitionManager.singleton.GoToScene(sceneIndex);
        }
        else
        {
            // Fallback if transition manager doesn't exist
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }
    }

}
