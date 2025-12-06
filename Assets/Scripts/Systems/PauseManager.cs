using UnityEngine;
using System;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;
    
    private bool isPaused = false;
    
    // Events
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f;
        
        OnGamePaused?.Invoke();
        
        Debug.Log("Game Paused");
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = 1f;
        
        OnGameResumed?.Invoke();
        
        Debug.Log("Game Resumed");
    }
    
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public bool IsPaused()
    {
        return isPaused;
    }
}

