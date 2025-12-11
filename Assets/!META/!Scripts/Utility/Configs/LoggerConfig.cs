using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
public class LoggerConfig : ScriptableObject
{
    [SerializeField] [Tooltip("Включить логирование")]
    private bool _isLoggingEnabled = true;

    [SerializeField] [Tooltip("Минимальный уровень логирования")]
    private LoggerUtility.LogLevel _minimumLogLevel = LoggerUtility.LogLevel.Info;

    [SerializeField] [Tooltip("Базовое имя файла лога (без расширения)")]
    private string _logFileName = "game_log";
    
    [SerializeField] [Tooltip("Путь для сохранения логов (по умолчанию - папка Logs в persistentDataPath)")]
    private string _logPath = "Logs";

    private const string TIME_FORMAT = "yyyy-MM-dd_HH:mm:ss";

    public bool IsLoggingEnabled => _isLoggingEnabled;
    public LoggerUtility.LogLevel MinimumLogLevel => _minimumLogLevel;
    public string LogFileName => _logFileName;

    public string GetFullLogFilePath()
    {
        var basePath = string.IsNullOrEmpty(_logPath)
            ? Application.persistentDataPath
            : Path.Combine(Application.persistentDataPath, _logPath);

        var dateTime = DateTime.Now.ToString(TIME_FORMAT);
        return Path.Combine(basePath, $"{_logFileName}_{dateTime}.txt");
    }
}
