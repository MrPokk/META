using System;
using System.IO;
using UnityEngine;

public static class LoggerUtility
{
    private static LoggerConfig s_config;
    private static string s_logPath;
    private static readonly object s_lock = new();
    private static string s_lastMessage;
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

                if (!s_config.isLoggingEnabled)
                    return;

                Directory.CreateDirectory(Path.GetDirectoryName(s_logPath));
                File.AppendAllText(s_logPath, $"=== Log started at {DateTime.Now} ===\n");
            }
            catch (Exception e) { Debug.LogError($"Logger init failed: {e.Message}"); }
        }
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        var logEntry = $"[{DateTime.Now.ToString(TIME_FORMAT)}] [{level}] {message}\n";

#if UNITY_EDITOR
        switch (level)
        {
            case LogLevel.Info: Debug.Log(logEntry); return;
            case LogLevel.Warning: Debug.LogWarning(logEntry); return;
            default: Debug.LogError(logEntry); return;
        }
#else
        if (level < s_config?.minimumLogLevel) return;
        if (message == s_lastMessage) return;

        s_lastMessage = message;

        if (s_config == null && !s_config.isLoggingEnabled)
            return;

        lock (s_lock)
        {
            try { File.AppendAllText(s_logPath, logEntry); }
            catch (Exception e) { Debug.LogError($"Log write failed: {e.Message}"); }
        }
#endif
    }

    public static void Info(string message) => Log(message, LogLevel.Info);
    public static void Warning(string message) => Log(message, LogLevel.Warning);
    public static void Error(string message) => Log(message, LogLevel.Error);
    public static void Critical(string message) => Log(message, LogLevel.Critical);
}
