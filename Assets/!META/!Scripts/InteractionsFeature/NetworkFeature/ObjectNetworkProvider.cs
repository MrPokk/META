using BitterECS.Core;
using BitterECS.Core.Integration;
using Mirror;
using System;
using UnityEngine;
using VContainer;

public class ObjectNetworkProvider : IProviderHandler
{
    private readonly SceneNetworkProvider _sceneNetworkProvider;
    private SceneNetworkProvider SceneProvider => _sceneNetworkProvider;

    [Inject]
    public ObjectNetworkProvider(SceneNetworkProvider sceneNetworkProvider)
    {
        _sceneNetworkProvider = sceneNetworkProvider;
    }

    #region Server Implementation

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SpawnObjectMessage>(OnServerSpawnObject);
        NetworkServer.RegisterHandler<DestroyObjectRequestMessage>(OnServerDestroyObject);
    }

    [Server]
    private void OnServerSpawnObject(NetworkConnectionToClient client, SpawnObjectMessage message)
    {
        if (!NetworkUtility.IsServerActive()) return;

        if (TrySetupEntity(message, out var spawnedEntity))
        {
            EcsLinker.Link(spawnedEntity.entity, spawnedEntity.view);
            NotifyClientsAboutSpawn(client, message);
        }
    }

    [Server]
    private void NotifyClientsAboutSpawn(NetworkConnectionToClient ownerClient, SpawnObjectMessage message)
    {
        var sceneType = SceneProvider.GetCurrentTypeSceneToClient(ownerClient);
        var sceneConnections = SceneProvider.GetConnectionsOnScene(sceneType);

        // Send owner message to the owner client
        var ownerMessage = new SpawnObjectMessage(
            ownerClient.connectionId,
            Type.GetType(message.entityTypeName),
            Type.GetType(message.viewTypeName),
            message.transformComponent);
        ownerClient.Send(ownerMessage);

        // Send observed messages to other clients
        foreach (var connection in sceneConnections)
        {
            if (connection == ownerClient)
                continue;

            var observedMessage = new SpawnObjectMessage(
                connection.connectionId,
                Type.GetType(message.entityTypeName),
                Type.GetType(message.viewTypeName),
                message.transformComponent);
            connection.Send(observedMessage);
        }
    }

    [Server]
    private void OnServerDestroyObject(NetworkConnectionToClient client, DestroyObjectRequestMessage message)
    {
        if (!NetworkUtility.IsServerActive()) return;
        // TODO: Implement object destruction logic
    }

    #endregion

    #region Client Implementation

    [Client]
    public static void RequestSpawnObject<TEntity, TView>(TransformComponent transform)
        where TEntity : EcsEntity
        where TView : EcsUnityView
    {
        var message = new SpawnObjectMessage(
            NetworkClient.connection.
            typeof(TEntity),
            typeof(TView),
            transform);

        CoroutineUtility.Run(
            NetworkUtility.WaitingToConnect(
                NetworkClient.connection,
                () => NetworkClient.Send(message)));
    }

    [Client]
    public static void RequestDestroyObject(NetworkIdentity target)
    {
        // TODO: Implement object destruction logic
    }

    [Client]
    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SpawnObjectMessage>(OnClientSpawnObject);
        NetworkClient.RegisterHandler<DestroyObjectRequestMessage>(OnClientDestroyObject);
    }

    [Client]
    private void OnClientSpawnObject(SpawnObjectMessage message)
    {
        if (!TrySetupEntity(message, out var result))
            return;

        var instance = UnityEngine.Object.Instantiate(result.view.gameObject);
        var transform = instance.transform;
        var msgTransform = message.transformComponent;

        transform.SetPositionAndRotation(msgTransform.position, msgTransform.rotation);
        transform.localScale = msgTransform.scale;

        result.entity.Get<NetworkSyncComponent>() = message.connectionId;
        EcsLinker.Link(result.entity, instance.GetComponent<ILinkableView>());
    }

    [Client]
    private void OnClientDestroyObject(DestroyObjectRequestMessage message)
    {
        // TODO: Implement client-side destruction logic
    }

    #endregion

    #region Helper Methods

    private bool TrySetupEntity(
        in SpawnObjectMessage message,
        out (EcsEntity entity, EcsUnityView view) result)
    {
        result = default;

        if (!TryResolveEntityTypes(message, out var entityType, out var viewType))
            return false;

        if (!TryCreateEntity(entityType, out var newEntity))
            return false;

        if (!newEntity.Has<NetworkSyncComponent>())
            return false;

        if (!TryGetNetworkView(viewType, out var viewPrefab))
            return false;

        result = (newEntity, viewPrefab);
        return true;
    }

    private bool TryResolveEntityTypes(
        in SpawnObjectMessage message,
        out Type entityType,
        out Type viewType)
    {
        entityType = Type.GetType(message.entityTypeName);
        viewType = Type.GetType(message.viewTypeName);

        if (entityType == null || viewType == null ||
            !entityType.IsSubclassOf(typeof(EcsEntity)) ||
            !viewType.IsSubclassOf(typeof(EcsUnityView)))
        {
            LoggerUtility.Error($"Failed to resolve types: Entity={message.entityTypeName}, View={message.viewTypeName}");
            return false;
        }

        return true;
    }

    private bool TryCreateEntity(Type entityType, out EcsEntity entity)
    {
        entity = (EcsEntity)Activator.CreateInstance(entityType);
        EcsWorld.GetToEntityType(entityType).AddEntity(entity);
        return true;
    }

    private static bool TryGetNetworkView(Type viewType, out EcsUnityView view)
    {
        return view = EcsUnityViewDatabase.GetPrefab(viewType) as EcsUnityView;
    }

    #endregion
}
