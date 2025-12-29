using UnityEngine;

[CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
public class LoggerConfig : ScriptableObject
{
    [SerializeField]
    [Tooltip("Minimum log level")]
    private LoggerUtility.LogLevel _minimumLogLevel = LoggerUtility.LogLevel.Info;

    [SerializeField]
    [Tooltip("Base log file name (without extension)")]
    private string _logFileName = "game_log";

    [SerializeField]
    [Tooltip("Path to save logs (default - Logs folder in persistentDataPath (For client))")]
    private string _logPathFolder = "logs";

#if UNITY_STANDALONE_LINUX
    public const string TIME_FORMAT = "yyyy-MM-dd_HH:mm:ss";
#elif UNITY_STANDALONE_WIN
    public const string TIME_FORMAT = "yyyy-MM-dd_HH-mm-ss";
#else
    public const string TIME_FORMAT = "yyyy-MM-dd_HH_mm_ss";
#endif

    public LoggerUtility.LogLevel MinimumLogLevel => _minimumLogLevel;
    public string LogFileName => _logFileName;
    public string LogPathFolder => _logPathFolder;
}
