using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public float invulnerabilityTime = 1f;
    public bool isInvulnerable = false;

    [Header("UI References")]
    public UnityEngine.UI.Slider healthBar;
    public UnityEngine.UI.Text healthText;

    private int currentHealth;
    private float invulnerabilityTimer = 0f;

    // Events for better decoupling
    public static event Action<int, int> OnHealthChanged; // current, max
    public static event Action OnPlayerDeath;
    public static event Action OnPlayerDamaged;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvulnerable || currentHealth <= 0) return;

        int damageAmount = Mathf.RoundToInt(damage);
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        
        Debug.Log($"Player took {damageAmount} damage! Health: {currentHealth}");
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnPlayerDamaged?.Invoke();

        if (currentHealth <= 0)
        {
            Debug.Log("Player has been defeated!");
            OnPlayerDeath?.Invoke();
            GameOver();
        }
        else
        {
            // Start invulnerability period
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityTime;
        }
    }

    public void Heal(int healAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Player healed for {healAmount}! Health: {currentHealth}");
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = (float)currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    void GameOver()
    {
        // Notify GameManager instead of directly controlling time
        if (GameManager.instance != null)
        {
            // GameManager will handle the game over logic
            Debug.Log("Game Over!");
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}
