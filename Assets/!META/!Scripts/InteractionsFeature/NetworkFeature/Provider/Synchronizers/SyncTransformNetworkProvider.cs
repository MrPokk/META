using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;

public class SyncTransformNetworkProvider : IProviderHandler
{
    public static void SendRequest(TransformComponent transform, SerializedType type, NetworkSyncComponent id = default) =>
        NetworkUtility.SendMessage<SyncTransformMessage>(new(id, transform, type));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SyncTransformMessage>(OnClientSyncTransform);
    }

    private void OnClientSyncTransform(SyncTransformMessage message)
    {
        foreach (var entity in GetRelevantEntities(message.entity.Type, message.connectionId.objectId))
        {
            ref var transform = ref entity.Get<TransformComponent>();
            transform = message.transformComponent;
            UpdateViewTransform(entity);
        }
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncTransformMessage>(OnServerSyncTransform);
    }

    private void OnServerSyncTransform(NetworkConnectionToClient client, SyncTransformMessage message)
    {
        foreach (var entity in GetRelevantEntities(message.entity.Type, message.connectionId.objectId))
        {
            entity.Get<TransformComponent>() = message.transformComponent;
            BroadcastTransformUpdate(client, message);
        }
    }

    private List<EcsEntity> GetRelevantEntities(SerializedType type, int objectId)
    {
        var results = new List<EcsEntity>();
        var filter = EcsWorld.GetToEntityType(type.Type)
            .Filter()
            .Include<NetworkSyncComponent>()
            .Include<TransformComponent>()
            .Exclude<ControllableComponent>()
            .Collect();

        foreach (var entity in filter)
        {
            if (entity.Get<NetworkSyncComponent>().objectId == objectId)
            {
                results.Add(entity);
            }
        }
        return results;
    }

    private void UpdateViewTransform(EcsEntity entity)
    {
        var view = (MonoBehaviour)EcsLinker.GetView(entity);
        var transform = entity.Get<TransformComponent>();
        view.transform.SetPositionAndRotation(transform.position, transform.rotation);
    }

    private void BroadcastTransformUpdate(NetworkConnectionToClient originalClient, SyncTransformMessage message)
    {
        foreach (var connection in ConnectionInfo.GetConnectionsInSameScene(originalClient))
        {
            if (connection != originalClient)
            {
                connection.Send(message);
            }
        }
    }
}
