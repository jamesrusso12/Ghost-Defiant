using System.Collections;
using UnityEngine;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    [Tooltip("Reference to WristMenuController for game stats display")]
    public WristMenuController wristUI;

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
    private bool isPaused = false;

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

    [Header("Game Flow")]
    [Tooltip("If true, game waits for LoadoutManager to start the rounds. If false, starts immediately.")]
    public bool enableIntroSequence = true;

    private void Start()
    {
        // Find WristUI if not assigned
        if (wristUI == null)
        {
            wristUI = FindFirstObjectByType<WristMenuController>();
            if (wristUI == null)
            {
                Debug.LogWarning("[GameManager] WristMenuController not found in scene!");
            }
        }
        
        if (!enableIntroSequence)
        {
            StartNextRound();
        }
    }

    public void StartGame()
    {
        if (!roundActive && currentRound == 0)
        {
        StartNextRound();
        }
    }
    

    private void Update()
    {
        if (roundActive && !isPaused)
        {
            timeRemaining -= Time.deltaTime;
            
            // Update timer on wrist UI
            if (wristUI != null)
            {
                wristUI.UpdateTimer(timeRemaining);
            }

            if (timeRemaining <= 0)
            {
                EndRound();
            }
        }
    }
    
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
    
    public bool IsPaused()
    {
        return isPaused;
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

        // Notify other systems about round start (WristUI listens to this event)
        OnRoundStarted?.Invoke(round);
        OnScoreChanged?.Invoke(totalGhostsKilled);

        ghostSpawner.BeginRound(round);
    }

    public void GhostKilled()
    {
        ghostsKilledThisRound++;
        totalGhostsKilled++;

        // Notify listeners (WristUI) about score change
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
        // Notify about round completion
        Debug.Log($"[GameManager] Round {currentRound} Complete!");

        yield return new WaitForSeconds(3f);

        StartNextRound();
    }

    void EndGame()
    {
        roundActive = false;
        ghostSpawner.StopSpawning();

        // Notify listeners about game end (WristUI will show end screen)
        OnGameEnded?.Invoke();
    }
    
    public int GetTotalScore()
    {
        return totalGhostsKilled;
    }

    public void AddScore(int points)
    {
        totalGhostsKilled += points;
        OnScoreChanged?.Invoke(totalGhostsKilled);
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
