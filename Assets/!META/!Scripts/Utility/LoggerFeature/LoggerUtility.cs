using System;
using System.IO;
using UnityEngine;

public static class LoggerUtility
{
    private static LoggerConfig s_config;
    private static string s_logPathToFile;
    private static readonly object s_lock = new();

    public enum LogLevel { Info, Warning, Error, Critical }

    public static void Initialize(LoggerConfig config, NetworkConfig networkConfig)
    {
        s_config = config;

#if UNITY_EDITOR
        return;
#else
        s_logPathToFile = GetFullLogFilePath(config, networkConfig);

        Debug.Log($"[Logger] init started log: [{s_logPathToFile}]");
        File.WriteAllText(s_logPathToFile, $"## Log started at {DateTime.Now}\n");
#endif
    }

    private static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (s_config == null)
        {
            throw new InvalidOperationException("Logger not initialized - call Initialize() first.");
        }

        var logEntry = $"[{DateTime.Now.ToString(LoggerConfig.TIME_FORMAT)}] [{level}] {message}\n";

#if UNITY_EDITOR
        switch (level)
        {
            case LogLevel.Info: Debug.Log(logEntry); break;
            case LogLevel.Warning: Debug.LogWarning(logEntry); break;
            default: Debug.LogError(logEntry); break;
        }

        return;
#else
        if (level < s_config.MinimumLogLevel)
        {
            return;
        }

        lock (s_lock)
        {
            try
            {
                File.AppendAllText(s_logPathToFile, logEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"Log write failed: {e.Message}");
            }
        }
#endif
    }


    private static string GetFullLogFilePath(LoggerConfig config, NetworkConfig networkConfig)
    {
        var combinedPath = networkConfig.NetworkType switch
        {
            NetworkType.Server => GetExecutableServerDirectory(config),
            NetworkType.Client => GetExecutableClientDirectory(config),
            _ => throw new ArgumentOutOfRangeException(nameof(networkConfig.NetworkType),
                $"Unsupported network type: {networkConfig.NetworkType}")
        };

        if (!Directory.Exists(combinedPath))
        {
            Directory.CreateDirectory(combinedPath);
        }

        var dateTime = DateTime.Now.ToString(LoggerConfig.TIME_FORMAT);
        return Path.Combine(combinedPath, $"{config.LogFileName}_{dateTime}.md");

    }

    private static string GetExecutableClientDirectory(LoggerConfig config)
    {
        return string.IsNullOrEmpty(config.LogPathFolder)
            ? Path.Combine(Application.persistentDataPath, "logs")
            : Path.Combine(Application.persistentDataPath, config.LogPathFolder);
    }

    private static string GetExecutableServerDirectory(LoggerConfig config)
    {
        var dataPath = Application.dataPath;
        var executableDir = Path.GetDirectoryName(dataPath);
        var logsPath = string.IsNullOrEmpty(config.LogPathFolder)
        ? Path.Combine(executableDir, "logs")
        : Path.Combine(executableDir, config.LogPathFolder);

        return logsPath;
    }

    public static void Info(string message, NetworkType network = NetworkType.None)
    {
        Log(network == NetworkType.None ? message : $"[{network}] {message}", LogLevel.Info);
    }

    public static void Warning(string message, NetworkType network = NetworkType.None)
    {
        Log(network == NetworkType.None ? message : $"[{network}] {message}", LogLevel.Warning);
    }

    public static void Error(string message, NetworkType network = NetworkType.None)
    {
        Log(network == NetworkType.None ? message : $"[{network}] {message}", LogLevel.Error);
    }

    public static void Critical(string message, NetworkType network = NetworkType.None)
    {
        Log(network == NetworkType.None ? message : $"[{network}] {message}", LogLevel.Critical);
    }
}
