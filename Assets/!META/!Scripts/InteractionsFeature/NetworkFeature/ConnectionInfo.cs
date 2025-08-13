using System.Collections.Generic;
using System.Linq;
using BitterECS.Core;
using Mirror;

public class ConnectionInfo : IServerConnected, IServerStart
{
    public Priority PrioritySystem => Priority.High;
    public static ConnectionInfo Instance { get; } = new();
    public static ushort GlobalObjectIdCounter = 0;
    public static readonly Dictionary<NetworkConnectionToClient, HashSet<EcsEntity>> ClientEntities = new();
    public static readonly Dictionary<NetworkConnectionToClient, SceneTypes> ClientToScene = new();
    public static readonly Dictionary<SceneTypes, HashSet<NetworkConnectionToClient>> SceneToConnections = new();


    public static IReadOnlyCollection<NetworkConnectionToClient> GetConnectionsInSameScene(NetworkConnectionToClient client)
    {
        if (ClientToScene.TryGetValue(client, out var scene))
        {
            return SceneToConnections.TryGetValue(scene, out var connections)
                ? connections.Where(c => c != client).ToList()
                : new List<NetworkConnectionToClient>();
        }
        return new List<NetworkConnectionToClient>();
    }

    public void Start()
    {
        GlobalObjectIdCounter = 0;
        ClientEntities.Clear();
        ClientToScene.Clear();
        SceneToConnections.Clear();
    }

    public void Connect(NetworkConnectionToClient client)
    {
        ClientEntities.TryAdd(client, new HashSet<EcsEntity>());
    }
}
