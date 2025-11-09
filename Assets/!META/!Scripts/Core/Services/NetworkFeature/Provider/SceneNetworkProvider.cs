using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneNetworkProvider : IProviderHandler
{
    public static void ChangeScene(SceneTypes sceneType) => NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);
    }

    private async void OnClientRequest(SceneChangeRequestMessage message)
    {
        await SceneLoader.LoadSceneAsync(message.sceneType, () => { NetworkUtility.SendMessage(new SceneTransitionCompleteMessage()); });
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
            LoggerUtility.Error($"Scene {message.sceneType} is not a server scene!");
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
            LoggerUtility.Error($"No scene type found for connection {client.connectionId}");
        }
    }

    private void MoveClientObjectsToScene(NetworkConnectionToClient client, SceneTypes sceneType)
    {
        var scene = SceneConfig.GetSceneToType(sceneType);
        if (!scene.IsValid())
        {
            LoggerUtility.Error($"Scene {sceneType} is not valid!");
            return;
        }

        if (!ConnectionInfo.PlayerEntityId.TryGetValue(client, out var playerEntity))
        {
            LoggerUtility.Warning($"No player entity id found for connection {client.connectionId}");
            return;
        }

        if (!NetworkServer.spawned.TryGetValue(playerEntity.netId, out var networkIdentity))
        {
            LoggerUtility.Error($"No network identity found for player entity id {playerEntity.netId}");
            return;
        }

        NetworkServer.RemovePlayerForConnection(client, RemovePlayerOptions.Unspawn);
        SceneManager.MoveGameObjectToScene(networkIdentity.gameObject, scene);
        NetworkServer.AddPlayerForConnection(client, networkIdentity.gameObject);

        SyncObjectSpawn(client, new SyncObjectSpawn(networkIdentity.netId));
    }

    private void SyncObjectSpawn(NetworkConnectionToClient client, SyncObjectSpawn spawn) => NetworkUtility.SendMessage(spawn, client);
}
