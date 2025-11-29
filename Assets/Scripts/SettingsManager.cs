using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Graphics Settings")]
    public TMP_Dropdown qualityDropdown;
    public Toggle vsyncToggle;
    public Slider renderScaleSlider;
    public TextMeshProUGUI renderScaleText;

    [Header("Gameplay Settings")]
    public Slider sensitivitySlider;
    public Toggle hapticToggle;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupUI();
    }

    private void SetupUI()
    {
        // Audio settings
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Graphics settings
        if (qualityDropdown != null)
        {
            qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        }

        if (vsyncToggle != null)
        {
            vsyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
            vsyncToggle.onValueChanged.AddListener(SetVSync);
        }

        if (renderScaleSlider != null)
        {
            renderScaleSlider.value = PlayerPrefs.GetFloat("RenderScale", 1f);
            renderScaleSlider.onValueChanged.AddListener(SetRenderScale);
            UpdateRenderScaleText();
        }

        // Gameplay settings
        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }

        if (hapticToggle != null)
        {
            hapticToggle.isOn = PlayerPrefs.GetInt("Haptics", 1) == 1;
            hapticToggle.onValueChanged.AddListener(SetHaptics);
        }
    }

    // Audio Settings
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        // Apply to SFX audio sources
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        // Apply to music audio sources
    }

    // Graphics Settings
    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt("QualityLevel", level);
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
    }

    public void SetRenderScale(float scale)
    {
        // Note: This would need XR-specific implementation
        PlayerPrefs.SetFloat("RenderScale", scale);
        UpdateRenderScaleText();
    }

    private void UpdateRenderScaleText()
    {
        if (renderScaleText != null)
        {
            renderScaleText.text = $"Render Scale: {renderScaleSlider.value:F2}x";
        }
    }

    // Gameplay Settings
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        // Apply to input systems
    }

    public void SetHaptics(bool enabled)
    {
        PlayerPrefs.SetInt("Haptics", enabled ? 1 : 0);
        // Apply to haptic feedback systems
    }

    private void LoadSettings()
    {
        // Load all settings from PlayerPrefs
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel()));
        QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSync", 1);
    }

    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
        SetupUI();
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}
