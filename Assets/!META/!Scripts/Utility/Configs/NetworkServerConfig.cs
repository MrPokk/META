#if UNITY_EDITOR
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Server Config")]
public class NetworkServerConfig : ScriptableObject
{
    [Header("KCP Server Settings")]
    public ushort kcpPort = 7777;
    public bool kcpNoDelay = true;
    public uint kcpInterval = 10;
    public int kcpFastResend = 2;

    [Header("WebSocket Server Settings")]
    public ushort webSocketPort = 8888;
    public bool webSocketSecure = false;
    [TextArea] public string webSocketSslCertJson = "";
    public int webSocketMaxMessageSize = 16384;

    public void ConfigureServer(NetworkManager manager)
    {
        if (manager.TryGetComponent<KcpTransport>(out var kcp))
        {
            kcp.Port = kcpPort;
            kcp.NoDelay = kcpNoDelay;
            kcp.Interval = kcpInterval;
            kcp.FastResend = kcpFastResend;
        }

        if (manager.TryGetComponent<SimpleWebTransport>(out var websocket))
        {
            websocket.port = webSocketPort;
            websocket.sslEnabled = webSocketSecure;
            websocket.sslCertJson = webSocketSslCertJson;
            websocket.maxMessageSize = webSocketMaxMessageSize;
        }
    }
}
#endif
