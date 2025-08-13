using System;
using System.Collections.Generic;
using BitterECS.Core;
using BitterECS.Core.Integration;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectNetworkProvider : IProviderHandler
{
    public static void SendRequest<TEntity, TView>(TransformComponent transform, NetworkSyncComponent id = default) =>
        NetworkUtility.SendMessage<SyncObjectSpawn>(new(id, typeof(TEntity), typeof(TView), transform));

    public void HandlersClient()
    {
        NetworkClient.ReplaceHandler<ForeignSpawnObjectMessage>(OnForeignClientSpawn);
        NetworkClient.RegisterHandler<OwnerSpawnObjectMessage>(OnOwnerClientSpawn);
    }

    private void OnForeignClientSpawn(ForeignSpawnObjectMessage message)
    {
        var entity = CreateEntity(message.entity.Type, message.view.Type, message.connectionId);
        entity.Remove<ControllableComponent>();
    }

    private void OnOwnerClientSpawn(OwnerSpawnObjectMessage message)
    {
        CreateEntity(message.entity.Type, message.view.Type, message.connectionId);
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncObjectSpawn>(OnServerSpawn);
    }

    private void OnServerSpawn(NetworkConnectionToClient client, SyncObjectSpawn message)
    {
        if (!message.entity.Type.IsSubclassOf(typeof(EcsEntity)))
        {
            LoggerUtility.Error("Type is not a subclass of EcsEntity");
            return;
        }

        var entity = CreateEntity(message.entity.Type, message.view.Type, new NetworkSyncComponent(client.connectionId));

        if (entity.Has<ControllableComponent>())
            entity.Remove<ControllableComponent>();
            
        ref var syncComponent = ref entity.Get<NetworkSyncComponent>();
        syncComponent.objectId = ConnectionInfo.GlobalObjectIdCounter++;

        var response = message;
        response.connectionId = syncComponent;
        ConnectionInfo.ClientEntities.GetOrAdd(client, _ => new() { entity }).Add(entity);

        SendSpawnResponses(client, response);
    }

    private void SendSpawnResponses(NetworkConnectionToClient ownerClient, SyncObjectSpawn message)
    {
        var sceneType = ConnectionInfo.ClientToScene.GetValueOrDefault(ownerClient);
        var connections = ConnectionInfo.SceneToConnections.GetValueOrDefault(sceneType);

        // Send owner-specific message to the owner
        ownerClient.Send(new OwnerSpawnObjectMessage(
            message.connectionId,
            message.entity,
            message.view,
            message.transformComponent
        ));

        // Broadcast regular message to other clients
        foreach (var connection in connections)
        {
            if (connection != ownerClient)
            {
                connection.Send(new ForeignSpawnObjectMessage(
                    message.connectionId,
                    message.entity,
                    message.view,
                    message.transformComponent
                ));
            }
        }
    }

    private EcsEntity CreateEntity(Type entityType, Type viewType, NetworkSyncComponent syncComponent)
    {
        var instance = EcsUnityViewDatabase.GetInstance(viewType);
        var entity = EcsWorld.GetToEntityType(entityType).AddEntity(entityType);

        entity.Get<NetworkSyncComponent>() = syncComponent;

        EcsLinker.Link(entity, instance);

        return entity;
    }
}
