using System.Collections.Generic;
using System.Linq;
using BitterECS.Core;
using Mirror;
using UnityEngine;

public class ConnectionInfo : IServerConnected, IServerStart
{
    public Priority PrioritySystem => Priority.High;
    public static ConnectionInfo Instance { get; } = new();
    public static readonly Dictionary<NetworkConnectionToClient, HashSet<GameObject>> ClientEntities = new();
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
        ClientEntities.TryAdd(client, new HashSet<GameObject>());
    }
}
