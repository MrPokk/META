using BitterECS.Core;
using Mirror;
using UnityEngine.SceneManagement;

public partial class SceneNetworkProvider : IProviderHandler
{
    public static void ChangeScene(SceneTypes sceneType) =>
    NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);
    }

    private async void OnClientRequest(SceneChangeRequestMessage message)
    {
        await SceneLoader.LoadSceneAsync(message.sceneType, onStart: TransitionStart, onComplete: TransitionComplete);
    }

    private static void TransitionStart()
    {
        EcsSystems.Run<IClientSceneTransitionStart>(system => system.OnStart());
    }

    private static void TransitionComplete()
    {
        NetworkUtility.SendMessage(new SceneTransitionCompleteMessage());
        EcsSystems.Run<IClientSceneTransitionComplete>(system => system.OnComplete());
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
        NetworkServer.RegisterHandler<SceneTransitionCompleteMessage>(OnServerTransitionComplete);
    }

    private void OnServerRequest(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        if (!SceneConfig.IsServerScene(message.sceneType))
        {
            LoggerUtility.Error($"Scene {message.sceneType} is not a server scene!", NetworkType.Server);
            return;
        }

        ConnectionInfo.ClientToScene[client] = message.sceneType;
        ConnectionInfo.SceneToConnections.GetOrAdd(message.sceneType, _ => new() { client }).Add(client);

        client.Send(new SceneChangeRequestMessage(message.sceneType));
    }

    private void OnServerTransitionComplete(NetworkConnectionToClient client, SceneTransitionCompleteMessage message)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(client, out var sceneType))
        {
            MoveClientObjectsToScene(client, sceneType);
        }
        else
        {
            LoggerUtility.Error($"No scene type found for connection {client.connectionId}", NetworkType.Server);
        }
    }

    private void MoveClientObjectsToScene(NetworkConnectionToClient client, SceneTypes sceneType)
    {
        var scene = SceneConfig.GetSceneToType(sceneType);
        if (!scene.IsValid())
        {
            LoggerUtility.Error($"Scene {sceneType} is not valid!", NetworkType.Server);
            return;
        }

        if (!ConnectionInfo.PlayerEntityId.TryGetValue(client, out var playerEntity))
        {
            LoggerUtility.Warning($"No player entity id found for connection {client.connectionId}", NetworkType.Server);
            return;
        }

        NetworkServer.RemovePlayerForConnection(client, RemovePlayerOptions.Unspawn);
        SceneManager.MoveGameObjectToScene(playerEntity.gameObject, scene);
        IsPlayerSpawnPoint.SetPositionPlayerToSpawnPoint(playerEntity, scene, out var position, out var rotation);
        NetworkServer.AddPlayerForConnection(client, playerEntity.gameObject);

        SyncObjectSpawn(client, new SyncObjectSpawn(playerEntity.netId, position, rotation));
    }

    private void SyncObjectSpawn(NetworkConnectionToClient client, SyncObjectSpawn spawn) => NetworkUtility.SendMessage(spawn, client);
}

