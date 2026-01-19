using System;
using System.IO;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Global Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("<size=18>Network Settings </size>")]
    [Header("<size=16> Common Settings </size>")]
    [SerializeField] private string _networkAddress = "localhost";
    [SerializeField] private NetworkType _networkType;
    [SerializeField] private NetworkMode _networkMode;

    [Header("<size=16>Transport Settings </size>")]
    [Header("KCP Server Settings")]
    [SerializeField]
    private KcpSettings _kcpSettings = new()
    {
        port = 7777,
        noDelay = true,
        interval = 10
    };

    [Header("WebSocket Server Settings")]
    [SerializeField]
    private WebSocketSettings _webSocketSettings = new()
    {
        port = 8888,
        secure = false,
        maxMessageSize = 16384,
        sendTimeout = 5000,
        receiveTimeout = 20000
    };

    [Header("<size=16>About Settings </size>")]
    [Header("Authentication")]
    [SerializeField] private NetworkAuthenticator _authenticator;

    [Header("Logging Settings")]
    [SerializeField] private bool _exceptionsDisconnect = true;

    public NetworkType NetworkType
    {
        get
        {
#if (DEDICATED_SERVER || SERVER || UNITY_SERVER || MIRROR_SERVER) && !UNITY_EDITOR
            return NetworkType.Server;
#else
            return _networkType;
#endif
        }
    }

    public void Configure(NetworkManager manager)
    {
        manager.networkAddress = _networkAddress;
        manager.authenticator = _authenticator;

        NetworkServer.exceptionsDisconnect = _exceptionsDisconnect;

        ConfigureTransport(manager);
    }

    private void ConfigureTransport(NetworkManager manager)
    {
        switch (_networkMode)
        {
            case NetworkMode.KCP:
                {
                    LoggerUtility.Info($"Transport using: {_networkMode}");
                    if (!manager.TryGetComponent<KcpTransport>(out var kcp))
                    {
                        throw LoggerUtility.Critical(" KcpTransport component not found");
                    }
                    kcp.Port = _kcpSettings.port;
                    kcp.NoDelay = _kcpSettings.noDelay;
                    kcp.Interval = _kcpSettings.interval;

                    break;
                }

            case NetworkMode.WEB:
                {
                    LoggerUtility.Info($"Transport using: {_networkMode}");
                    if (!manager.TryGetComponent<SimpleWebTransport>(out var websocket))
                    {
                        throw LoggerUtility.Critical("SimpleWebTransport component not found");
                    }

                    websocket.port = _webSocketSettings.port;
                    websocket.sslEnabled = _webSocketSettings.secure;
                    websocket.sslCertJson = _webSocketSettings.sslCertJson;
                    websocket.maxMessageSize = _webSocketSettings.maxMessageSize;
                    websocket.sendTimeout = _webSocketSettings.sendTimeout;
                    websocket.receiveTimeout = _webSocketSettings.receiveTimeout;

                    break;
                }

            default:
                throw LoggerUtility.Critical($"Network mode {_networkMode} is not supported", NetworkType.Server);
        }
    }

    public void SaveToFile(string filePath)
    {
        LoggerUtility.Info($"Saving network config to {filePath}", NetworkType.Server);
        var configData = CreateConfigData();
        var json = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            LoggerUtility.Warning($"Network config file not found at {filePath}", NetworkType.Server);
            return;
        }

        var json = File.ReadAllText(filePath);
        var configData = JsonConvert.DeserializeObject<NetworkConfigData>(json);

        ApplyConfigData(configData);
    }

    public void LoadOrSaveServerConfig()
    {
        var configPath = GetServerConfigPath();
        var configDir = Path.GetDirectoryName(configPath);

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        if (File.Exists(configPath))
        {
            LoadFromFile(configPath);
            LoggerUtility.Info($"Loaded server config from: {configPath}", NetworkType.Server);
        }
        else
        {
            SaveToFile(configPath);
            LoggerUtility.Info($"Created new server config at: {configPath}", NetworkType.Server);
        }
    }

    private string GetServerConfigPath()
    {
        var dataPath = Application.dataPath;
        var executableDir = Path.GetDirectoryName(dataPath);
        return Path.Combine(executableDir, "config", "server_config.json");
    }

    private NetworkConfigData CreateConfigData() => new()
    {
        networkAddress = _networkAddress,
        kcpSettings = _kcpSettings,
        webSocketSettings = _webSocketSettings,
        exceptionsDisconnect = _exceptionsDisconnect
    };

    private void ApplyConfigData(NetworkConfigData configData)
    {
        if (configData == null) return;

        if (!string.IsNullOrEmpty(configData.networkAddress))
            _networkAddress = configData.networkAddress;

        if (configData.kcpSettings != null)
            _kcpSettings = configData.kcpSettings.Value;

        if (configData.webSocketSettings != null)
            _webSocketSettings = configData.webSocketSettings.Value;

        if (configData.exceptionsDisconnect.HasValue)
            _exceptionsDisconnect = configData.exceptionsDisconnect.Value;
    }


    [Serializable]
    private class NetworkConfigData
    {
        public string networkAddress;
        public KcpSettings? kcpSettings;
        public WebSocketSettings? webSocketSettings;
        public bool? exceptionsDisconnect;
    }

    [Serializable]
    private struct KcpSettings
    {
        public ushort port;
        public bool noDelay;
        public uint interval;
    }

    [Serializable]
    private struct WebSocketSettings
    {
        public ushort port;
        public bool secure;
        public string sslCertJson;
        public int maxMessageSize;
        public int sendTimeout;
        public int receiveTimeout;
    }
}
