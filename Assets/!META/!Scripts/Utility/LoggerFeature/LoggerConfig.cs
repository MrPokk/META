using UnityEngine;

[CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logging/Logger Config")]
public class LoggerConfig : ScriptableObject
{
    [SerializeField]
    [Tooltip("Minimum log level")]
    private LoggerUtility.LogLevel _minimumLogLevel = LoggerUtility.LogLevel.Info;

    [SerializeField]
    [Tooltip("Base log file name (without extension)")]
    private string _logFileName = "latest";

    [SerializeField]
    [Tooltip("Path to save logs (default - Logs folder in persistentDataPath (For client))")]
    private string _logPathFolder = "logs";

    [SerializeField]
    [Tooltip("Maximum log file size in MB before rotation")]
    private float _maxLogSizeMB = 10f;

    [SerializeField]
    [Tooltip("Maximum number of archived logs to keep")]
    private int _maxArchivedLogs = 10;

    [SerializeField]
    [Tooltip("Compress archived logs (using gzip)")]
    private bool _compressArchivedLogs = true;

    public const string TIME_FORMAT_FILE_NAME = "yyyy-MM-dd";
    public const string TIME_FORMAT_LOG = "HH:mm:ss";

    public LoggerUtility.LogLevel MinimumLogLevel => _minimumLogLevel;
    public string LogFileName => _logFileName;
    public string LogPathFolder => _logPathFolder;
    public float MaxLogSizeMB => _maxLogSizeMB;
    public int MaxArchivedLogs => _maxArchivedLogs;
    public bool CompressArchivedLogs => _compressArchivedLogs;
}
