using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;

public class SyncSceneNetworkProvider : IProviderHandler
{
    public Priority PrioritySystem => Priority.High;

    public static void SendRequest() => NetworkUtility.SendMessage<SyncStateSceneMessage>(new());

    public void HandlersClient()
    {
        NetworkClient.ReplaceHandler<SyncStateSceneMessage>(OnClientSync);
    }

    private void OnClientSync(SyncStateSceneMessage message)
    { }

    public void HandlersServer()
    {
        NetworkServer.ReplaceHandler<SyncStateSceneMessage>(OnServerSync);
    }

    private void OnServerSync(NetworkConnectionToClient client, SyncStateSceneMessage message)
    {
        var sceneType = ConnectionInfo.ClientToScene.GetValueOrDefault(client);
        var connections = ConnectionInfo.SceneToConnections.GetValueOrDefault(sceneType);

        if (connections == null)
            return;

        foreach (var connection in connections)
        {
            if (connection == client)
                continue;

            var entities = ConnectionInfo.ClientEntities.GetValueOrDefault(connection);
            if (entities == null)
                continue;

            foreach (var entity in entities)
            {
                var NonOwnerSpawnObjectMessage = new ForeignSpawnObjectMessage(
                    entity.Get<NetworkSyncComponent>(),
                    entity.GetType(),
                    EcsLinker.GetView(entity).GetType(),
                    entity.Get<TransformComponent>());

                client.Send(NonOwnerSpawnObjectMessage);
            }
        }
    }

}
