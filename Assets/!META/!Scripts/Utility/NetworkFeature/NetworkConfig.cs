using System;
using System.IO;
using System.Security.Authentication;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using Newtonsoft.Json;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Global Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("<size=18>Network Settings </size>")]
    [SerializeField] private NetworkManager _networkManager;
    public NetworkManager NetworkPrefab => _networkManager;

    [Header("<size=16>Common Settings </size>")]
    [SerializeField] private string _networkAddress = "localhost";
    [SerializeField] private NetworkType _networkType;

    [Header("<size=16>Transport Settings </size>")]
    [Header("KCP Server Settings")]
    [SerializeField]
    private KcpSettings _kcpSettings = new()
    {
        port = 7777,
        dualMode = true,
        noDelay = true,
        interval = 10,
        timeout = 10000,
        recvBufferSize = 7361536,
        sendBufferSize = 7361536,
        fastResend = 2,
        receiveWindowSize = 4096,
        sendWindowSize = 4096,
        maxRetransmit = 40,
        maximizeSocketBuffers = true,
        reliableMaxMessageSize = 297433,
        unreliableMaxMessageSize = 1194
    };

    [Header("WebSocket Server Settings")]
    [SerializeField]
    private WebSocketSettings _webSocketSettings = new()
    {
        port = 8888,
        secure = false,
        sslProtocols = SslProtocols.Tls12,
        sslCertJson = "./cert.json",
        maxMessageSize = 16384,
        maxHandshakeSize = 16384,
        serverMaxMsgsPerTick = 10000,
        clientMaxMsgsPerTick = 1000,
        sendTimeout = 5000,
        receiveTimeout = 20000,
        noDelay = true,
        batchSend = false,
        waitBeforeSend = false
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

    private void OnValidate()
    {
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager is not assigned in NetworkConfig.");
            return;
        }
    }

    public void Configure(NetworkManager manager)
    {
        if (manager == null)
        {
            throw LoggerUtility.Critical("NetworkManager is not assigned in NetworkConfig.");
        }

        manager.networkAddress = _networkAddress;
        manager.authenticator = _authenticator;

        NetworkServer.exceptionsDisconnect = _exceptionsDisconnect;

        ConfigureTransport(manager);
    }

    private void ConfigureTransport(NetworkManager manager)
    {
        SetupKcp(manager);
        SetupWebSocket(manager);
    }

    private void SetupWebSocket(NetworkManager manager)
    {
        if (!manager.TryGetComponent<SimpleWebTransport>(out var websocket))
        {
            throw LoggerUtility.Critical("SimpleWebTransport component not found");
        }

        websocket.port = _webSocketSettings.port;
        websocket.sslEnabled = _webSocketSettings.secure;
        websocket.sslProtocols = _webSocketSettings.sslProtocols;
        websocket.sslCertJson = _webSocketSettings.sslCertJson;
        websocket.maxMessageSize = _webSocketSettings.maxMessageSize;
        websocket.maxHandshakeSize = _webSocketSettings.maxHandshakeSize;
        websocket.serverMaxMsgsPerTick = _webSocketSettings.serverMaxMsgsPerTick;
        websocket.clientMaxMsgsPerTick = _webSocketSettings.clientMaxMsgsPerTick;
        websocket.sendTimeout = _webSocketSettings.sendTimeout;
        websocket.receiveTimeout = _webSocketSettings.receiveTimeout;
        websocket.noDelay = _webSocketSettings.noDelay;
        websocket.batchSend = _webSocketSettings.batchSend;
        websocket.waitBeforeSend = _webSocketSettings.waitBeforeSend;
    }

    private void SetupKcp(NetworkManager manager)
    {
        if (!manager.TryGetComponent<KcpTransport>(out var kcp))
        {
            throw LoggerUtility.Critical("KcpTransport component not found");
        }

        kcp.Port = _kcpSettings.port;
        kcp.DualMode = _kcpSettings.dualMode;
        kcp.NoDelay = _kcpSettings.noDelay;
        kcp.Interval = _kcpSettings.interval;
        kcp.Timeout = _kcpSettings.timeout;
        kcp.RecvBufferSize = _kcpSettings.recvBufferSize;
        kcp.SendBufferSize = _kcpSettings.sendBufferSize;
        kcp.FastResend = _kcpSettings.fastResend;
        kcp.ReceiveWindowSize = _kcpSettings.receiveWindowSize;
        kcp.SendWindowSize = _kcpSettings.sendWindowSize;
        kcp.MaxRetransmit = _kcpSettings.maxRetransmit;
        kcp.MaximizeSocketBuffers = _kcpSettings.maximizeSocketBuffers;

        // Эти параметры могут быть вычислены на основе других настроек
        kcp.ReliableMaxMessageSize = _kcpSettings.reliableMaxMessageSize;
        kcp.UnreliableMaxMessageSize = _kcpSettings.unreliableMaxMessageSize;
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
        public bool dualMode;
        public bool noDelay;
        public uint interval;
        public int timeout;
        public int recvBufferSize;
        public int sendBufferSize;
        public int fastResend;
        public uint receiveWindowSize;
        public uint sendWindowSize;
        public uint maxRetransmit;
        public bool maximizeSocketBuffers;
        public int reliableMaxMessageSize;
        public int unreliableMaxMessageSize;
    }

    [Serializable]
    private struct WebSocketSettings
    {
        public ushort port;
        public bool secure;
        public SslProtocols sslProtocols;
        public string sslCertJson;
        public int maxMessageSize;
        public int maxHandshakeSize;
        public int serverMaxMsgsPerTick;
        public int clientMaxMsgsPerTick;
        public int sendTimeout;
        public int receiveTimeout;
        public bool noDelay;
        public bool batchSend;
        public bool waitBeforeSend;
    }
}
