using UnityEngine;
using System.IO;
using System;

/// <summary>
/// Writes all Unity Debug.Log messages to a file on Quest
/// File location: /sdcard/Android/data/[your.package.name]/files/game_debug.txt
/// </summary>
public class FileLogger : MonoBehaviour
{
    private StreamWriter logWriter;
    private string logFilePath;
    public static FileLogger Instance;
    public void LogExternal(string message) => LogToFile(message);


    void Awake()
    {
        // Create log file in persistent data path (accessible from Quest)
        logFilePath = Path.Combine(Application.persistentDataPath, "game_debug.txt");

        try
        {
            logWriter = new StreamWriter(logFilePath, false); // false = overwrite each session
            logWriter.AutoFlush = true; // IMPORTANT: AutoFlush ensures data is written immediately, accessible via file explorer

            // Subscribe to Unity's log callback
            Application.logMessageReceived += HandleLog;

            LogToFile($"===== GAME DEBUG LOG STARTED =====");
            LogToFile($"Time: {DateTime.Now}");
            LogToFile($"Log file: {logFilePath}");
            LogToFile($"=====================================");

            Debug.Log($"[FileLogger] Logging to: {logFilePath}");
            Debug.Log($"[FileLogger] Access via: This PC → Quest → Internal shared storage → Android → data → com.Russo.VRprojectGame → files → game_debug.txt");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FileLogger] Failed to create log file: {e.Message}");
        }

        Instance = this;

    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // FILTER: Skip MRUK World Lock warnings - they're SDK spam, not performance issues
        if (logString.Contains("EnableWorldLock is enabled and is controlling the tracking space position"))
        {
            return; // Skip this log entry entirely
        }

        // Log messages from bullet system, loadout system, performance, AND lighting debugging
        if (
            logString.Contains("[GunScript]") ||
            logString.Contains("[ProjectileController]") ||
            logString.Contains("[DestructibleGlobalMeshManager]") ||
            logString.Contains("[LoadoutManager]") ||
            logString.Contains("[Performance]") ||
            logString.Contains("[PERF]") ||
            logString.Contains("MRUK") ||
            logString.Contains("Light") ||
            logString.Contains("Shadow") ||
            logString.Contains("OVR") ||
            logString.Contains("Exposure") ||

            // LightingDiagnostics tags
            logString.Contains("[LightingDiagnostics]") ||
            logString.Contains("[ENV]") ||
            logString.Contains("[LIGHTS]") ||
            logString.Contains("[LIGHT]") ||
            logString.Contains("[CAM]") ||
            logString.Contains("[PIPE]") ||
            logString.Contains("[PIPE-URP]") ||
            logString.Contains("[PT]")
)
        {
            string timestamp = Time.time.ToString("F2");
            LogToFile($"[{timestamp}s] {type}: {logString}");

            if (type == LogType.Error && !string.IsNullOrEmpty(stackTrace))
            {
                LogToFile($"  Stack: {stackTrace}");
            }
        }

    }

    void LogToFile(string message)
    {
        if (logWriter != null)
        {
            logWriter.WriteLine(message);
        }
    }

    void OnDestroy()
    {
        if (logWriter != null)
        {
            LogToFile($"===== LOG SESSION ENDED =====");
            Application.logMessageReceived -= HandleLog;
            logWriter.Close();
        }
    }

    void OnApplicationQuit()
    {
        if (logWriter != null)
        {
            LogToFile($"===== APPLICATION QUIT =====");
            Application.logMessageReceived -= HandleLog;
            logWriter.Close();
        }
    }
}
