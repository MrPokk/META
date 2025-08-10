using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;

public class ConnectionNetworkProvider : IServerConnected
{
    public Priority PrioritySystem => Priority.High;
    public static ConnectionNetworkProvider Instance { get; } = new();

    private readonly HashSet<NetworkConnection> _networkConnections = new();

    public NetworkConnection GetConnection(NetworkConnection conn)
    {
        if (_networkConnections.TryGetValue(conn, out var connection))
        {
            return connection;
        }
        LoggerUtility.Error("Could not find connection: " + conn.ToString());
        return null;
    }

    public void Connect(NetworkConnectionToClient client)
    {
        Instance._networkConnections.Add(client);
    }
}
