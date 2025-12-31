using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LoggerUtility
{
    private static LoggerConfig s_config;
    private static string s_logPathToFile;
    private static long s_maxLogSizeBytes;
    private static readonly object s_lock = new();

    public enum LogLevel { Info, Warning, Error, Critical }

    public static void Initialize(LoggerConfig config, NetworkConfig networkConfig)
    {
        s_config = config;
        s_maxLogSizeBytes = (long)(config.MaxLogSizeMB * 1024 * 1024);

#if UNITY_EDITOR
        return;
#else
        s_logPathToFile = GetFullLogFilePath(config, networkConfig);
        
        Debug.Log($"[Logger] init started log: [{s_logPathToFile}]");
        
        ArchiveExistingLogOnStart(s_logPathToFile);
        File.WriteAllText(s_logPathToFile, $"## Log started at {DateTime.Now}\n");
        CleanupOldArchives(s_config, Path.GetDirectoryName(s_logPathToFile));
#endif
    }

    private static void Log(string message, LogLevel level = LogLevel.Info)
    {
        if (s_config == null)
        {
            throw new InvalidOperationException("Logger not initialized - call Initialize() first.");
        }

        var logEntry = $"[{DateTime.Now.ToString(LoggerConfig.TIME_FORMAT_LOG)}] [{level}] {message}\n";

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
                CheckAndRotateLogBySize(s_logPathToFile);
                
                File.AppendAllText(s_logPathToFile, logEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"Log write failed: {e.Message}");
            }
        }
#endif
    }

    private static void ArchiveExistingLogOnStart(string logPath)
    {
        try
        {
            if (!File.Exists(logPath))
            {
                return;
            }

            var logDir = Path.GetDirectoryName(logPath);
            var logName = Path.GetFileNameWithoutExtension(logPath);
            
            var fileInfo = new FileInfo(logPath);
            if (fileInfo.Length == 0)
            {
                File.Delete(logPath);
                return;
            }

            var dateString = DateTime.Now.ToString(LoggerConfig.TIME_FORMAT_FILE_NAME);
            var archiveNumber = 1;
            
            while (File.Exists(Path.Combine(logDir, $"{dateString}-{archiveNumber}.log")) ||
                   File.Exists(Path.Combine(logDir, $"{dateString}-{archiveNumber}.log.gz")))
            {
                archiveNumber++;
            }

            var archivedLogName = $"{dateString}-{archiveNumber}.log";
            var archivedLogPath = Path.Combine(logDir, archivedLogName);
            
            File.Move(logPath, archivedLogPath);
            Debug.Log($"[Logger] Archived existing log to: {archivedLogName} (Size: {fileInfo.Length} bytes)");
            
            if (s_config.CompressArchivedLogs)
            {
                CompressLogFile(archivedLogPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to archive existing log on startup: {e.Message}");
        }
    }

    private static void CheckAndRotateLogBySize(string logPath)
    {
        try
        {
            if (!File.Exists(logPath))
            {
                return;
            }

            var fileInfo = new FileInfo(logPath);
            if (fileInfo.Length < s_maxLogSizeBytes)
            {
                return;
            }

            var logDir = Path.GetDirectoryName(logPath);
            var dateString = DateTime.Now.ToString(LoggerConfig.TIME_FORMAT_FILE_NAME);
            var archiveNumber = 1;
            
            while (File.Exists(Path.Combine(logDir, $"{dateString}-{archiveNumber}.log")) ||
                   File.Exists(Path.Combine(logDir, $"{dateString}-{archiveNumber}.log.gz")))
            {
                archiveNumber++;
            }

            var archivedLogName = $"{dateString}-{archiveNumber}.log";
            var archivedLogPath = Path.Combine(logDir, archivedLogName);
            
            File.Move(logPath, archivedLogPath);
            
            if (s_config.CompressArchivedLogs)
            {
                CompressLogFile(archivedLogPath);
            }
            
            File.WriteAllText(logPath, $"## Log rotated (size exceeded {s_config.MaxLogSizeMB}MB) at {DateTime.Now}\n");
            
            Debug.Log($"[Logger] Rotated log due to size limit to: {archivedLogName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Log rotation by size failed: {e.Message}");
        }
    }

    private static void CompressLogFile(string logPath)
    {
        try
        {
            using (var originalFileStream = File.OpenRead(logPath))
            using (var compressedFileStream = File.Create(logPath + ".gz"))
            using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }

            File.Delete(logPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Log compression failed: {e.Message}");
        }
    }

    private static void CleanupOldArchives(LoggerConfig config, string logDir)
    {
        try
        {
            var allArchives = new List<string>();

            var logFiles = Directory.GetFiles(logDir, "*.log")
                .Where(f => !f.EndsWith(Path.DirectorySeparatorChar + $"{config.LogFileName}.log", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var compressedFiles = Directory.GetFiles(logDir, "*.log.gz").ToList();

            allArchives.AddRange(logFiles);
            allArchives.AddRange(compressedFiles);

            allArchives.Sort((a, b) => 
                File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            while (allArchives.Count > config.MaxArchivedLogs)
            {
                var fileToDelete = allArchives[0];
                File.Delete(fileToDelete);
                Debug.Log($"[Logger] Removed old archive: {Path.GetFileName(fileToDelete)}");
                allArchives.RemoveAt(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Archive cleanup failed: {e.Message}");
        }
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

        return Path.Combine(combinedPath, $"{config.LogFileName}.log");
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
