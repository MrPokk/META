using System.Collections.Generic;
using BitterECS.Core;
using Mirror;

public class ConnectionInfo : IServerConnected, IServerDisconnected, IServerStart
{
    public Priority PrioritySystem => Priority.High;
    public static ConnectionInfo Instance { get; } = new();
    public static readonly Dictionary<NetworkConnectionToClient, HashSet<NetworkIdentity>> ClientEntities = new();
    public static readonly Dictionary<NetworkConnectionToClient, NetworkIdentity> PlayerEntityId = new();
    public static readonly Dictionary<NetworkConnectionToClient, SceneTypes> ClientToScene = new();
    public static readonly Dictionary<SceneTypes, HashSet<NetworkConnectionToClient>> SceneToConnections = new();

    public void Start()
    {
        ClientEntities.Clear();
        ClientToScene.Clear();
        SceneToConnections.Clear();
    }

    public void Connect(NetworkConnectionToClient client)
    {
        ClientEntities.TryAdd(client, new HashSet<NetworkIdentity>());
    }

    public void Disconnect(NetworkConnectionToClient client)
    {
        if (ClientEntities.TryGetValue(client, out var objects))
        {
            foreach (var networkIdentity in objects)
            {
                if (networkIdentity == null)
                {
                    continue;
                }
                
                NetworkServer.Destroy(networkIdentity.gameObject);
            }
        }

        ClientEntities.Remove(client);
        PlayerEntityId.Remove(client);
        ClientToScene.Remove(client);

        foreach (var sceneConnections in SceneToConnections.Values)
        {
            sceneConnections.Remove(client);
        }

        LoggerUtility.Info($"Cleaned up connection info for connection {client.connectionId}", NetworkType.Server);
    }
}
