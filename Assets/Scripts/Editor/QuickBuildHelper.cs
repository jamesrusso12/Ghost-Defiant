using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;

/// <summary>
/// Quick build helper for MR/Passthrough development.
/// Provides one-click build shortcuts with optimal settings for testing.
/// </summary>
public class QuickBuildHelper : EditorWindow
{
    private static string lastBuildTime = "";
    
    [MenuItem("Tools/Quick Build/Development Build and Run %#b", false, 1)]
    public static void QuickDevelopmentBuild()
    {
        Debug.Log("=== STARTING QUICK DEVELOPMENT BUILD ===");
        
        // Save all scenes
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        
        // Configure for development
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
        EditorUserBuildSettings.allowDebugging = true;
        
        // Build and run
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetScenePaths();
        buildOptions.locationPathName = GetBuildPath();
        buildOptions.target = BuildTarget.Android;
        buildOptions.options = BuildOptions.Development | BuildOptions.AutoRunPlayer;
        
        DateTime startTime = DateTime.Now;
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        DateTime endTime = DateTime.Now;
        TimeSpan buildDuration = endTime - startTime;
        
        lastBuildTime = buildDuration.ToString(@"mm\:ss");
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✓ BUILD SUCCEEDED in {lastBuildTime}");
            Debug.Log($"✓ Build size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log("✓ Installing to device...");
        }
        else
        {
            Debug.LogError($"✗ BUILD FAILED: {report.summary.result}");
        }
    }
    
    [MenuItem("Tools/Quick Build/Release Build (Optimized)", false, 2)]
    public static void QuickReleaseBuild()
    {
        Debug.Log("=== STARTING RELEASE BUILD (OPTIMIZED) ===");
        
        // Save all scenes
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        
        // Configure for release
        EditorUserBuildSettings.development = false;
        EditorUserBuildSettings.connectProfiler = false;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
        EditorUserBuildSettings.allowDebugging = false;
        
        // Build
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = GetScenePaths();
        buildOptions.locationPathName = GetBuildPath();
        buildOptions.target = BuildTarget.Android;
        buildOptions.options = BuildOptions.None;
        
        DateTime startTime = DateTime.Now;
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        DateTime endTime = DateTime.Now;
        TimeSpan buildDuration = endTime - startTime;
        
        lastBuildTime = buildDuration.ToString(@"mm\:ss");
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✓ RELEASE BUILD SUCCEEDED in {lastBuildTime}");
            Debug.Log($"✓ Build size: {report.summary.totalSize / (1024 * 1024)} MB");
            Debug.Log($"✓ APK location: {buildOptions.locationPathName}");
            
            // Offer to install
            if (EditorUtility.DisplayDialog(
                "Build Complete",
                $"Release build completed successfully!\n\nBuild time: {lastBuildTime}\nSize: {report.summary.totalSize / (1024 * 1024)} MB\n\nInstall to connected device?",
                "Install Now",
                "Later"))
            {
                // Install via ADB
                string adbPath = GetAdbPath();
                if (!string.IsNullOrEmpty(adbPath))
                {
                    System.Diagnostics.Process.Start(adbPath, $"install -r \"{buildOptions.locationPathName}\"");
                    Debug.Log("✓ Installing release build to device...");
                }
            }
        }
        else
        {
            Debug.LogError($"✗ BUILD FAILED: {report.summary.result}");
        }
    }
    
    [MenuItem("Tools/Quick Build/Build Settings Window", false, 20)]
    public static void OpenBuildSettings()
    {
        EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
    }
    
    [MenuItem("Tools/Quick Build/Show Build Info", false, 21)]
    public static void ShowBuildInfo()
    {
        string info = "=== BUILD INFORMATION ===\n\n";
        info += $"Target: {EditorUserBuildSettings.activeBuildTarget}\n";
        info += $"Development Build: {EditorUserBuildSettings.development}\n";
        info += $"Script Debugging: {EditorUserBuildSettings.allowDebugging}\n";
        info += $"Deep Profiling: {EditorUserBuildSettings.buildWithDeepProfilingSupport}\n";
        info += $"\nScenes in Build:\n";
        
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                info += $"  ✓ {scene.path}\n";
            }
        }
        
        if (!string.IsNullOrEmpty(lastBuildTime))
        {
            info += $"\nLast Build Time: {lastBuildTime}\n";
        }
        
        EditorUtility.DisplayDialog("Build Information", info, "OK");
    }
    
    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
    
    private static string GetBuildPath()
    {
        // Use project folder for builds
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string buildFolder = Path.Combine(projectPath, "Builds");
        
        // Create builds folder if it doesn't exist
        if (!Directory.Exists(buildFolder))
        {
            Directory.CreateDirectory(buildFolder);
        }
        
        // Generate filename with timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"{Application.productName}_{timestamp}.apk";
        
        return Path.Combine(buildFolder, filename);
    }
    
    private static string GetAdbPath()
    {
        // Try to find ADB in Android SDK
        string androidSdkRoot = EditorPrefs.GetString("AndroidSdkRoot");
        if (string.IsNullOrEmpty(androidSdkRoot))
        {
            return null;
        }
        
        string adbPath = Path.Combine(androidSdkRoot, "platform-tools", "adb.exe");
        if (File.Exists(adbPath))
        {
            return adbPath;
        }
        
        return null;
    }
}

/// <summary>
/// Build configuration validator - provides menu item to check build settings.
/// </summary>
public class BuildValidator
{
    [MenuItem("Tools/Quick Build/Validate Build Settings", false, 22)]
    public static void ValidateBuildSettings()
    {
        // Check for common issues
        bool hasIssues = false;
        string issues = "=== BUILD VALIDATION ===\n\n";
        
        // Check if any scene is in build settings
        if (EditorBuildSettings.scenes.Length == 0)
        {
            issues += "❌ No scenes added to build settings!\n";
            hasIssues = true;
        }
        else
        {
            issues += $"✓ {EditorBuildSettings.scenes.Length} scene(s) in build\n";
        }
        
        // Check if development build is disabled (might want it for testing)
        if (!EditorUserBuildSettings.development)
        {
            issues += "\n⚠️ Development Build is OFF\n";
            issues += "  (Enable for debug console and better error messages)\n";
        }
        else
        {
            issues += "\n✓ Development Build is ON\n";
        }
        
        // Check for VR/MR settings
        if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
        {
            issues += "\n❌ Min SDK version is low (Quest requires API 29+)\n";
            issues += $"  Current: {PlayerSettings.Android.minSdkVersion}\n";
            hasIssues = true;
        }
        else
        {
            issues += $"\n✓ Min SDK version: {PlayerSettings.Android.minSdkVersion}\n";
        }
        
        // Check build target
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            issues += "\n⚠️ Build target is not Android!\n";
            issues += $"  Current: {EditorUserBuildSettings.activeBuildTarget}\n";
        }
        else
        {
            issues += "\n✓ Build target: Android\n";
        }
        
        if (hasIssues)
        {
            issues += "\n⚠️ ISSUES FOUND - Please fix before building!";
            EditorUtility.DisplayDialog("Build Validation - Issues Found", issues, "OK");
        }
        else
        {
            issues += "\n✓ All checks passed! Ready to build.";
            EditorUtility.DisplayDialog("Build Validation - Passed", issues, "OK");
        }
        
        Debug.Log(issues);
    }
}
