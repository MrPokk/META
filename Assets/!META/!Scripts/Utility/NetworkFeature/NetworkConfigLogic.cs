using Mirror;
using Mirror.SimpleWeb;
using kcp2k;
using UnityEngine;

public static class NetworkConfigLogic
{
    public static void ApplyToManager(NetworkConfig config, NetworkManager manager)
    {
        if (config == null || manager == null) return;

        manager.networkAddress = string.IsNullOrEmpty(config.NetworkAddress)
            ? "localhost"
            : config.NetworkAddress;

        manager.maxConnections = config.MaxConnections;
        manager.authenticator = config.Authenticator;

        NetworkServer.exceptionsDisconnect = config.ExceptionsDisconnect;

        ApplyTransportConfig(config, manager);
        LogConfiguration(manager);
    }

    private static void ApplyTransportConfig(NetworkConfig config, NetworkManager manager)
    {
        if (manager.TryGetComponent<KcpTransport>(out var kcp))
        {
            config.KcpConfig.ApplyTo(kcp);
        }
        else
        {
            throw LoggerUtility.Critical("KcpTransport component not found");
        }

        if (manager.TryGetComponent<SimpleWebTransport>(out var websocket))
        {
            config.WebSocketConfig.ApplyTo(websocket);
        }
        else
        {
            throw LoggerUtility.Critical("SimpleWebTransport component not found");
        }
    }

    private static void LogConfiguration(NetworkManager manager)
    {
#if !UNITY_EDITOR
        LoggerUtility.Info($"Network Address: {manager.networkAddress}");

        if (manager.TryGetComponent<KcpTransport>(out var kcp))
            LoggerUtility.Info($"KCP Port: {kcp.Port}");

        if (manager.TryGetComponent<SimpleWebTransport>(out var websocket))
            LoggerUtility.Info($"WebSocket Port: {websocket.port}");

        LoggerUtility.Info($"Max Connections: {manager.maxConnections}");
#endif
    }
}
