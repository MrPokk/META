using System.Collections.Generic;
using System.Linq;
using BitterECS.Core;
using Mirror;
using VContainer;

public class SyncSceneNetworkProvider : IProviderHandler
{
    private readonly SceneNetworkProvider _sceneNetworkProvider;
    private SceneNetworkProvider SceneProvider => _sceneNetworkProvider;

    [Inject]
    public SyncSceneNetworkProvider(SceneNetworkProvider sceneNetworkProvider)
    {
        _sceneNetworkProvider = sceneNetworkProvider;
    }

    #region Server Methods

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncStateSceneMessage>(OnServerSyncStateScene);
    }

    [Server]
    private void OnServerSyncStateScene(NetworkConnectionToClient client, SyncStateSceneMessage message)
    {
        var sceneType = SceneProvider.GetCurrentTypeSceneToClient(client);
        if (sceneType == SceneTypes.None)
            return;

        var entitiesOnScene = SceneProvider.GetEntitiesOnScene(sceneType);
        if (!entitiesOnScene.Any())
            return;

        SendInitialStateToClient(client, entitiesOnScene);
    }

    private void SendInitialStateToClient(NetworkConnectionToClient client, IReadOnlyCollection<EcsEntity> entities)
    {
        foreach (var entity in entities)
        {
            if (!entity.Has<TransformComponent>())
                continue;

            ref var transformComponent = ref entity.Get<TransformComponent>();
            var view = EcsLinker.GetView(entity);

            if (view == null)
                continue;

            client.Send(new SpawnObjectMessage(
                entity.GetType(),
                view.GetType(),
                transformComponent
            ));
        }
    }

    #endregion

    #region Client Methods

    [Client]
    public void HandlersClient()
    {}

    [Client]
    public static void SyncStateScene()
    {
        CoroutineUtility.Run(
        NetworkUtility.WaitingToConnect(
        NetworkClient.connection, () => NetworkClient.Send(new SyncStateSceneMessage())));
    }

    #endregion
}
