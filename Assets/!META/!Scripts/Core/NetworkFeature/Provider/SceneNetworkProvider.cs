using System.Threading.Tasks;
using BitterECS.Core;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneNetworkProvider : IProviderHandler
{
    public static async UniTask ChangeScene(SceneTypes sceneType) =>
    await NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient() =>
    NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);

    private async void OnClientRequest(SceneChangeRequestMessage message) =>
    await SceneLoader.LoadSceneAsync(message.sceneType, onStart: TransitionStart, onComplete: TransitionComplete);

    private static void TransitionStart() =>
    EcsSystems.Run<IClientSceneTransitionStart>(system => system.OnStart());

    private static async void TransitionComplete()
    {
        await NetworkUtility.SendMessage(new SceneTransitionCompleteMessage());
        EcsSystems.Run<IClientSceneTransitionComplete>(system => system.OnComplete());
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
        NetworkServer.RegisterHandler<SceneTransitionCompleteMessage>(OnServerTransitionComplete);
    }

    private async void OnServerRequest(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        if (!SceneConfig.IsServerScene(message.sceneType))
        {
            LoggerUtility.Error($"Scene {message.sceneType} is not a server scene!", NetworkType.Server);
            return;
        }

        ConnectionInfo.ClientToScene[client] = message.sceneType;
        ConnectionInfo.SceneToConnections.GetOrAdd(message.sceneType, _ => new() { client }).Add(client);
        LoggerUtility.Info($"Server requested scene change to {message.sceneType} for client {client.connectionId}", NetworkType.Server);

        await NetworkUtility.MessagingService.SendMessage(new SceneChangeRequestMessage(message.sceneType), client);
    }

    private async void OnServerTransitionComplete(NetworkConnectionToClient client, SceneTransitionCompleteMessage message)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(client, out var sceneType))
        {
            await MoveClientObjectsToScene(client, sceneType);
        }
        else
        {
            LoggerUtility.Error($"No scene type found for connection {client.connectionId}", NetworkType.Server);
        }
    }

    private async UniTask MoveClientObjectsToScene(NetworkConnectionToClient client, SceneTypes sceneType)
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

        await SyncObjectSpawn(client, new SyncObjectSpawn(playerEntity.netId, position, rotation));
    }

    private async UniTask SyncObjectSpawn(NetworkConnectionToClient client, SyncObjectSpawn spawn) =>
    await NetworkUtility.SendMessage(spawn, client);
}

