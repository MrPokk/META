
using BitterECS.Core;
using Mirror;
using UnityEngine;
using VContainer;

public class SyncTransformNetworkProvider : IProviderHandler
{
    private readonly SceneNetworkProvider _sceneNetworkProvider;
    private SceneNetworkProvider SceneProvider => _sceneNetworkProvider;

    [Inject]
    public SyncTransformNetworkProvider(SceneNetworkProvider sceneNetworkProvider)
    {
        _sceneNetworkProvider = sceneNetworkProvider;
    }


    #region Server Methods

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncTransformMessage>(OnServerSyncTransform);
    }

    [Server]
    private void OnServerSyncTransform(NetworkConnectionToClient client, SyncTransformMessage message)
    {
        var sceneType = SceneProvider.GetCurrentTypeSceneToClient(client);
        foreach (var connection in SceneProvider.GetConnectionsOnScene(sceneType))
        {
            if (connection != client)
                connection.Send(message);
        }
    }

    #endregion

    #region Client Methods

    [Client]
    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SyncTransformMessage>(OnClientSyncTransform);
    }

    public static void SyncTransformToClient(TransformComponent transform, int entityId)
    {
        CoroutineUtility.Run(
        NetworkUtility.WaitingToConnect(
        NetworkClient.connection, () => NetworkClient.Send(new SyncTransformMessage(entityId, transform))));
    }

    [Client]
    private void OnClientSyncTransform(SyncTransformMessage message)
    {
        var presenter = EcsWorld.Get<EcsObservedPresenter>();
        var entities = presenter.Filter()
            .Include<TransformComponent>()
            .Include<ViewComponent>()
            .Include<NetworkSyncComponent>()
            .Collect();

        UpdateEntitiesTransforms(entities, message);
    }

    private void UpdateEntitiesTransforms(EcsFilter.FilterEnumerator entities, SyncTransformMessage message)
    {
        foreach (var entity in entities)
        {
            if (entity.Get<NetworkSyncComponent>().ID != message.entityId)
                continue;

            ref var transformComponent = ref entity.Get<TransformComponent>();
            ref var viewComponent = ref entity.Get<ViewComponent>();

            var ecsUnityView = (MonoBehaviour)viewComponent.current;
            if (ecsUnityView == null)
                continue;

            ecsUnityView.transform.position = message.transformComponent.position;
            ecsUnityView.transform.rotation = message.transformComponent.rotation;
        }
    }

    #endregion
}
