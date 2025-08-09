using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Global Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("<size=18>Network Settings </size>")]

    [Header("<size=16> Common Settings </size>")]
    public string networkAddress = "localhost";
    public NetworkType networkType;

    [Header("<size=16>Transport Settings </size>")]

    [Header("KCP Server Settings")]
    public ushort kcpPort = 7777;
    public bool kcpNoDelay = true;
    public uint kcpInterval = 10;

    [Header("WebSocket Server Settings")]
    public ushort webSocketPort = 8888;
    public bool webSocketSecure = false;
    [TextArea] public string webSocketSslCertJson = "";
    public int webSocketMaxMessageSize = 16384;
    public int webSocketSendTimeout = 5000;
    public int webSocketReceiveTimeout = 20000;

    [Header("<size=16>About Settings </size>")]

    [Header("Authentication")]
    public NetworkAuthenticator authenticator;

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public bool autoCreatePlayer = true;
    public PlayerSpawnMethod playerSpawnMethod = PlayerSpawnMethod.RoundRobin;

    [Header("Security")]
    public bool exceptionsDisconnect = true;

    [Header("Snapshot Interpolation")]
    public float snapshotInterval = 0.1f;
    public float snapshotMinRate = 0.01f;
    public float snapshotMaxRate = 0.5f;

    [Header("Connection Quality")]
    public float evaluationInterval = 1f;

    public void Configure(NetworkManager manager)
    {
        manager.networkAddress = networkAddress;
        manager.authenticator = authenticator;
        manager.playerPrefab = playerPrefab;
        manager.autoCreatePlayer = autoCreatePlayer;
        manager.playerSpawnMethod = playerSpawnMethod;

        NetworkServer.exceptionsDisconnect = exceptionsDisconnect;   
        

        // Настройки транспорта KCP
        if (manager.TryGetComponent<KcpTransport>(out var kcp))
        {
            kcp.Port = kcpPort;
            kcp.NoDelay = kcpNoDelay;
            kcp.Interval = kcpInterval;
        }

        // Настройки WebSocket транспорта
        if (manager.TryGetComponent<SimpleWebTransport>(out var websocket))
        {
            websocket.port = webSocketPort;
            websocket.sslEnabled = webSocketSecure;
            websocket.sslCertJson = webSocketSslCertJson;
            websocket.maxMessageSize = webSocketMaxMessageSize;
            websocket.sendTimeout = webSocketSendTimeout;
            websocket.receiveTimeout = webSocketReceiveTimeout;
        }
    }
}
