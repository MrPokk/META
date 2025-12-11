using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Global Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("<size=18>Network Settings </size>")]

    [Header("<size=16> Common Settings </size>")]
    [SerializeField] private string _networkAddress = "localhost";
    [SerializeField] private NetworkType _networkType;

    [Header("<size=16>Transport Settings </size>")]

    [Header("KCP Server Settings")]
    [SerializeField] private ushort _kcpPort = 7777;
    [SerializeField] private bool _kcpNoDelay = true;
    [SerializeField] private uint _kcpInterval = 10;

    [Header("WebSocket Server Settings")]
    [SerializeField] private ushort _webSocketPort = 8888;
    [SerializeField] private bool _webSocketSecure = false;
    [SerializeField] [TextArea] private string _webSocketSslCertJson = "";
    [SerializeField] private int _webSocketMaxMessageSize = 16384;
    [SerializeField] private int _webSocketSendTimeout = 5000;
    [SerializeField] private int _webSocketReceiveTimeout = 20000;

    [Header("<size=16>About Settings </size>")]

    [Header("Authentication")]
    [SerializeField] private NetworkAuthenticator _authenticator;

    [Header("Player Settings")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private bool _autoCreatePlayer = true;
    [SerializeField] private PlayerSpawnMethod _playerSpawnMethod = PlayerSpawnMethod.RoundRobin;

    [Header("Security")]
    [SerializeField] private bool _exceptionsDisconnect = true;

    public NetworkType NetworkType => _networkType;

    public void Configure(NetworkManager manager)
    {
        manager.networkAddress = _networkAddress;
        manager.authenticator = _authenticator;
        manager.playerPrefab = _playerPrefab;
        manager.autoCreatePlayer = _autoCreatePlayer;
        manager.playerSpawnMethod = _playerSpawnMethod;

        NetworkServer.exceptionsDisconnect = _exceptionsDisconnect;

        // Настройки транспорта KCP
        if (manager.TryGetComponent<KcpTransport>(out var kcp))
        {
            kcp.Port = _kcpPort;
            kcp.NoDelay = _kcpNoDelay;
            kcp.Interval = _kcpInterval;
        }

        // Настройки WebSocket транспорта
        if (manager.TryGetComponent<SimpleWebTransport>(out var websocket))
        {
            websocket.port = _webSocketPort;
            websocket.sslEnabled = _webSocketSecure;
            websocket.sslCertJson = _webSocketSslCertJson;
            websocket.maxMessageSize = _webSocketMaxMessageSize;
            websocket.sendTimeout = _webSocketSendTimeout;
            websocket.receiveTimeout = _webSocketReceiveTimeout;
        }
    }
}
