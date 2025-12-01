using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI fpsText;
    public TextMeshProUGUI memoryText;
    public Slider fpsSlider;

    [Header("Settings")]
    public float updateInterval = 0.5f;
    public int targetFPS = 90;

    private float accum = 0.0f;
    private int frames = 0;
    private float timeleft;
    private float fps;

    void Start()
    {
        timeleft = updateInterval;
        
        // Set target frame rate for VR
        Application.targetFrameRate = targetFPS;
        
        // Enable VSync for better frame pacing
        QualitySettings.vSyncCount = 1;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0f)
        {
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0.0f;
            frames = 0;

            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {fps:F1}";
            
            // Color code based on performance
            if (fps >= targetFPS * 0.9f)
                fpsText.color = Color.green;
            else if (fps >= targetFPS * 0.7f)
                fpsText.color = Color.yellow;
            else
                fpsText.color = Color.red;
        }

        if (memoryText != null)
        {
            long memoryUsage = System.GC.GetTotalMemory(false);
            memoryText.text = $"Memory: {memoryUsage / (1024 * 1024)} MB";
        }

        if (fpsSlider != null)
        {
            fpsSlider.value = fps / targetFPS;
        }
    }

    public float GetCurrentFPS()
    {
        return fps;
    }

    public bool IsPerformanceGood()
    {
        return fps >= targetFPS * 0.8f;
    }
}
