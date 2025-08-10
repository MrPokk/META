using System;
using BitterECS.Core;
using BitterECS.Core.Integration;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class ObjectNetworkProvide : IHandlerMessages
{
    private readonly NetworkObjectPrefabConfig _prefabConfig;
    private readonly SceneNetworkProvider _sceneNetworkProvider;

    [Inject]
    public ObjectNetworkProvide(NetworkObjectPrefabConfig prefabConfig, SceneNetworkProvider sceneNetworkProvider)
    {
        _prefabConfig = prefabConfig;
        _sceneNetworkProvider = sceneNetworkProvider;
    }

    #region Server

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SpawnObjectMessage>(OnServerSpawnObject);
        NetworkServer.RegisterHandler<DestroyObjectRequestMessage>(OnServerDestroyObject);
    }

    [Server]
    private void OnServerSpawnObject(NetworkConnectionToClient client, SpawnObjectMessage message)
    {
        if (!NetworkServer.active)
        {
            LoggerUtility.Error("NetworkServer is not active");
            return;
        }

        if (!_prefabConfig.ContainsPrefab(message.prefabId))
        {
            LoggerUtility.Error($"Prefab with ID '{message.prefabId}' not registered");
            return;
        }

    }

    [Server]
    private void OnServerDestroyObject(NetworkConnectionToClient client, DestroyObjectRequestMessage message)
    {
        if (!NetworkServer.active)
        {
            LoggerUtility.Error("NetworkServer is not active");
            return;
        }

        if (NetworkServer.spawned.TryGetValue(message.netId, out var networkIdentity))
        {
            NetworkServer.Destroy(networkIdentity.gameObject);
        }

    }

    #endregion

    #region Client

    [Client]
    public static void ClientRequestSpawnObject<TEntity, TView>(SpawnObjectMessage message) where TEntity : EcsEntity where TView : EcsNetworkView
    {

        var typeEntity = typeof(TEntity);
        var typeView = typeof(TView);

        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(NetworkClient.connection, () => OnClientSpanObject(message)));
    }

    [Client]
    public static void ClientRequestDestroyObject(uint netId)
    {
        NetworkClient.Send(new DestroyObjectRequestMessage { netId = netId });
    }

    [Client]
    public void HandlersClient()
    {
        _prefabConfig.RegisterAllPrefabs();
    }
    #endregion
}
