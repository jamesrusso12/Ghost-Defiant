using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class XRConfigurationManager : MonoBehaviour
{
    [Header("XR Settings")]
    public bool enableXROnStart = true;
    public bool useOpenXR = true;
    public bool useOculusProvider = false;
    
    [Header("Performance Settings")]
    public int targetFrameRate = 90;
    public bool enableVSync = true;
    public float renderScale = 1.0f;
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    
    private XRGeneralSettings xrSettings;
    private XRManagerSettings xrManagerSettings;
    
    void Awake()
    {
        // Get XR settings
        xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            xrSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        }
        
        xrManagerSettings = xrSettings.Manager;
        if (xrManagerSettings == null)
        {
            xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            xrSettings.Manager = xrManagerSettings;
        }
    }
    
    void Start()
    {
        if (enableXROnStart)
        {
            InitializeXR();
        }
        
        // Configure performance settings
        ConfigurePerformanceSettings();
    }
    
    private void InitializeXR()
    {
        if (enableDebugLogs)
            Debug.Log("[XRConfigurationManager] Initializing XR...");
            
        // Stop any existing XR
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
        
        // Initialize XR
        StartCoroutine(InitializeXRCoroutine());
    }
    
    private System.Collections.IEnumerator InitializeXRCoroutine()
    {
        // Wait a frame for XR to be ready
        yield return null;
        
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            
            if (enableDebugLogs)
                Debug.Log("[XRConfigurationManager] XR initialized successfully!");
                
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            
            // Log active XR providers
            LogActiveXRProviders();
        }
        else
        {
            Debug.LogError("[XRConfigurationManager] XR settings not found!");
        }
    }
    
    private void LogActiveXRProviders()
    {
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            var activeLoaders = XRGeneralSettings.Instance.Manager.activeLoaders;
            if (enableDebugLogs)
            {
                Debug.Log($"[XRConfigurationManager] Active XR Loaders: {activeLoaders.Count}");
                foreach (var loader in activeLoaders)
                {
                    Debug.Log($"[XRConfigurationManager] - {loader.name}");
                }
            }
        }
    }
    
    private void ConfigurePerformanceSettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Configure VSync
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;
        
        // Configure render scale (this would need XR-specific implementation)
        if (enableDebugLogs)
        {
            Debug.Log($"[XRConfigurationManager] Performance settings: FPS={targetFrameRate}, VSync={enableVSync}, RenderScale={renderScale}");
        }
    }
    
    public void RestartXR()
    {
        if (enableDebugLogs)
            Debug.Log("[XRConfigurationManager] Restarting XR...");
            
        if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            
            StartCoroutine(InitializeXRCoroutine());
        }
    }
    
    public bool IsXRInitialized()
    {
        return XRGeneralSettings.Instance != null && 
               XRGeneralSettings.Instance.Manager != null && 
               XRGeneralSettings.Instance.Manager.isInitializationComplete;
    }
    
    public void SetRenderScale(float scale)
    {
        renderScale = Mathf.Clamp(scale, 0.5f, 2.0f);
        
        // This would need XR-specific implementation
        // For Meta Quest, you'd use OVRManager.instance.fixedFoveatedRenderingLevel
        if (enableDebugLogs)
            Debug.Log($"[XRConfigurationManager] Render scale set to: {renderScale}");
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Pause XR when app is paused
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                XRGeneralSettings.Instance.Manager.StopSubsystems();
            }
        }
        else
        {
            // Resume XR when app is resumed
            if (XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null)
            {
                XRGeneralSettings.Instance.Manager.StartSubsystems();
            }
        }
    }
}
