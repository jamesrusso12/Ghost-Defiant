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

    void Awake()
    {
        // Create log file in persistent data path (accessible from Quest)
        logFilePath = Path.Combine(Application.persistentDataPath, "game_debug.txt");

        try
        {
            logWriter = new StreamWriter(logFilePath, false); // false = overwrite each session
            logWriter.AutoFlush = true;

            // Subscribe to Unity's log callback
            Application.logMessageReceived += HandleLog;

            LogToFile($"===== GAME DEBUG LOG STARTED =====");
            LogToFile($"Time: {DateTime.Now}");
            LogToFile($"Log file: {logFilePath}");
            LogToFile($"=====================================");

            Debug.Log($"[FileLogger] Logging to: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FileLogger] Failed to create log file: {e.Message}");
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Log messages from bullet system AND loadout system
        if (logString.Contains("[GunScript]") ||
            logString.Contains("[ProjectileController]") ||
            logString.Contains("[DestructibleGlobalMeshManager]") ||
            logString.Contains("[LoadoutManager]") ||
            logString.Contains("MRUK"))
        {
            string timestamp = Time.time.ToString("F2");
            LogToFile($"[{timestamp}s] {type}: {logString}");
            
            // Also log stack trace for errors
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
