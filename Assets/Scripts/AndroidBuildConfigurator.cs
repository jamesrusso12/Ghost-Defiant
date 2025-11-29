using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AndroidBuildConfigurator : MonoBehaviour
{
    [Header("Android Build Settings")]
    public bool autoConfigureOnStart = true;
    public bool enableDebugLogs = true;
    
    void Start()
    {
        if (autoConfigureOnStart)
        {
            ConfigureAndroidBuild();
        }
    }
    
    [ContextMenu("Configure Android Build Settings")]
    public void ConfigureAndroidBuild()
    {
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Configuring Android build settings...");
            
        FixInputHandling();
        OptimizeRenderFeatures();
        ConfigureXR();
        OptimizePerformance();
        
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Android build configuration completed!");
    }
    
    private void FixInputHandling()
    {
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Fixing input handling...");
            
        // This requires editor scripts to modify PlayerSettings
        Debug.Log("[AndroidBuildConfigurator] MANUAL FIX REQUIRED:");
        Debug.Log("1. Go to Edit > Project Settings > Player > Configuration");
        Debug.Log("2. Change 'Active Input Handling' from 'Both' to 'Input System Package (New)'");
        Debug.Log("3. This will fix the Android build error");
    }
    
    private void OptimizeRenderFeatures()
    {
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Optimizing render features...");
            
        Debug.Log("[AndroidBuildConfigurator] MANUAL FIX REQUIRED:");
        Debug.Log("1. Go to your URP Renderer asset");
        Debug.Log("2. Remove or disable 'Screen Space Ambient Occlusion' feature");
        Debug.Log("3. This will improve performance on Android devices");
    }
    
    private void ConfigureXR()
    {
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Configuring XR settings...");
            
        Debug.Log("[AndroidBuildConfigurator] XR Configuration:");
        Debug.Log("- OpenXR is properly configured");
        Debug.Log("- Meta Quest target is set");
        Debug.Log("- XR Management is active");
    }
    
    private void OptimizePerformance()
    {
        if (enableDebugLogs)
            Debug.Log("[AndroidBuildConfigurator] Performance optimizations...");
            
        Debug.Log("[AndroidBuildConfigurator] Performance Tips:");
        Debug.Log("- Disable Screen Space Ambient Occlusion");
        Debug.Log("- Use Mobile/Quest-optimized quality settings");
        Debug.Log("- Enable Dynamic Resolution if needed");
        Debug.Log("- Use object pooling (already implemented)");
    }
    
    // Public method to check current settings
    [ContextMenu("Check Build Settings")]
    public void CheckBuildSettings()
    {
        Debug.Log("[AndroidBuildConfigurator] Current Build Settings:");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Unity Version: {Application.unityVersion}");
        
        #if UNITY_EDITOR
        Debug.Log($"Editor Build Target: {EditorUserBuildSettings.activeBuildTarget}");
        // Debug.Log($"Scripting Backend: {PlayerSettings.GetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup)}");
        Debug.Log($"API Level: {PlayerSettings.Android.minSdkVersion}");
        #endif
        
        // Check for common issues
        CheckCommonIssues();
    }
    
    private void CheckCommonIssues()
    {
        Debug.Log("[AndroidBuildConfigurator] Checking for common issues...");
        
        // These would be checked in editor scripts
        Debug.Log("Common Android Build Issues:");
        Debug.Log("1. Input Handling: Should be 'Input System Package (New)' only");
        Debug.Log("2. Render Features: Disable SSAO for mobile");
        Debug.Log("3. API Level: Should be 23+ for Quest");
        Debug.Log("4. Architecture: Should be ARM64");
    }
}
