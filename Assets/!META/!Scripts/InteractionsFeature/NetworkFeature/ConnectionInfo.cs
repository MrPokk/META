using System.Collections.Generic;
using BitterECS.Core;
using Mirror;

public class ConnectionInfo : IServerConnected, IServerStart
{
    public Priority PrioritySystem => Priority.High;
    public static ConnectionInfo Instance { get; } = new();
    private readonly HashSet<NetworkConnection> _networkConnections = new();
    public static readonly Dictionary<NetworkConnectionToClient, HashSet<EcsEntity>> ClientEntities = new();
    public static readonly Dictionary<NetworkConnectionToClient, SceneTypes> ClientSceneTypes = new();
    public static readonly Dictionary<SceneTypes, HashSet<NetworkConnectionToClient>> SceneToConnections = new();

    public void Start()
    {
        ClientEntities.Clear();
        ClientSceneTypes.Clear();
        SceneToConnections.Clear();
    }

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
