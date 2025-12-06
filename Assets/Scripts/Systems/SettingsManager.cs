using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;
    
    [Header("Volume Settings")]
    public Volume postProcessVolume;
    
    private float masterVolume = 1f;
    private float brightness = 1f;
    
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
        
        // Load saved settings
        LoadSettings();
    }
    
    void Start()
    {
        // Find post-process volume if not assigned
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
        }
        
        // Apply loaded settings
        ApplyVolume(masterVolume);
        ApplyBrightness(brightness);
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume(masterVolume);
        SaveSettings();
    }
    
    public void SetBrightness(float brightnessValue)
    {
        brightness = Mathf.Clamp01(brightnessValue);
        ApplyBrightness(brightness);
        SaveSettings();
    }
    
    void ApplyVolume(float volume)
    {
        AudioListener.volume = volume;
    }
    
    void ApplyBrightness(float brightnessValue)
    {
        // Adjust ambient light
        RenderSettings.ambientIntensity = Mathf.Lerp(0.5f, 1.5f, brightnessValue);
        
        // If using URP post-processing
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            // Try to adjust color adjustments or exposure
            if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-1f, 1f, brightnessValue);
            }
        }
    }
    
    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("Brightness", brightness);
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        brightness = PlayerPrefs.GetFloat("Brightness", 1f);
    }
    
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    public float GetBrightness()
    {
        return brightness;
    }
}
