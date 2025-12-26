using System;
using System.IO;
using UnityEngine;

public static class LoggerUtility
{
    private static LoggerConfig s_config;
    private static string s_logPath;
    private static readonly object s_lock = new();
    private const string TIME_FORMAT = "HH:mm:ss";

    public enum LogLevel { Info, Warning, Error, Critical }

    public static void Initialize(LoggerConfig config)
    {
        lock (s_lock)
        {
            try
            {
                s_config = config;
                s_logPath = config.GetFullLogFilePath();

                if (!s_config.IsLoggingEnabled)
                {
                    return;
                }

                Debug.Log($"[Logger] init started log: [{s_logPath}]");
                File.Create(s_logPath).Close();
                File.AppendAllText(s_logPath, $"## Log started at {DateTime.Now}\n");
            }
            catch (Exception e) { Debug.LogError($"Logger init failed: {e.Message}"); }
        }
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {

        if (s_config == null)
        {
            return;
        }

        var logEntry = $"[{DateTime.Now.ToString(TIME_FORMAT)}] [{level}] {message}\n";

#if UNITY_EDITOR
        switch (level)
        {
            case LogLevel.Info: Debug.Log(logEntry); break;
            case LogLevel.Warning: Debug.LogWarning(logEntry); break;
            default: Debug.LogError(logEntry); break;
        }
#endif
        if (!s_config.IsLoggingEnabled)
        {
            return;
        }

        if (level < s_config.MinimumLogLevel)
        {
            return;
        }

        lock (s_lock)
        {
            try { File.AppendAllText(s_logPath, logEntry); }
            catch (Exception e) { Debug.LogError($"Log write failed: {e.Message}"); }
        }
    }

    public static void Info(string message) => Log(message, LogLevel.Info);
    public static void Warning(string message) => Log(message, LogLevel.Warning);
    public static void Error(string message) => Log(message, LogLevel.Error);
    public static void Critical(string message) => Log(message, LogLevel.Critical);
}
