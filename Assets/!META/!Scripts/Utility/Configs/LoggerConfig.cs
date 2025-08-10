using System;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
public class LoggerConfig : ScriptableObject
{

    [Tooltip("Включить логирование")]
    public bool isLoggingEnabled = true;

    [Tooltip("Минимальный уровень логирования")]
    public LoggerUtility.LogLevel minimumLogLevel = LoggerUtility.LogLevel.Info;

    [Tooltip("Базовое имя файла лога (без расширения)")]
    public string logFileName = "game_log";

    [Tooltip("Путь для сохранения логов (по умолчанию - папка Logs в persistentDataPath)")]
    public string logPath = "Logs";

    private const string TIME_FORMAT = "yyyy-MM-dd_HH:mm:ss";

    public string GetFullLogFilePath()
    {
        var basePath = string.IsNullOrEmpty(logPath)
            ? Application.persistentDataPath
            : Path.Combine(Application.persistentDataPath, logPath);

        var dateTime = DateTime.Now.ToString(TIME_FORMAT);
        return Path.Combine(basePath, $"{logFileName}_{dateTime}.txt");
    }
}
