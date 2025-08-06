using UnityEngine;

[CreateAssetMenu(fileName = "NetworkClientConfig", menuName = "Network/Client Config")]
public class NetworkClientConfig : ScriptableObject
{
    [Header("Connection Settings - KCP")]
    public string serverIP = "127.0.0.1";
    public ushort kcpPort = 7777;

    [Header("Connection Settings - WebSocket")]
    public string webSocketServerIP = "localhost";
    public ushort webSocketPort = 8888;
    public bool webSocketSecure = false;

    [Header("Reconnection Settings")]
    [Range(1, 60)] public float reconnectDelay = 5f;
    [Range(1, 10)] public int maxReconnectAttempts = 3;

    [Header("Performance Settings")]
    public int messageBufferSize = 4096;

    public string GetConnectionAddress(bool isWebGL)
    {
        if (isWebGL)
        {
            string protocol = webSocketSecure ? "wss" : "ws";
            return $"{protocol}://{webSocketServerIP}:{webSocketPort}";
        }
        else
        {
            return $"{serverIP}:{kcpPort}";
        }
    }
}

