using Mirror;
using UnityEngine.SceneManagement;

public class SceneNetworkProvider : IProviderHandler
{
    public static void ChangeScene(SceneTypes sceneType) => NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);
    }

    private async void OnClientRequest(SceneChangeRequestMessage message)
    {
        await SceneLoader.LoadSceneAsync(message.sceneType, () => OnSceneLoaded(message.sceneType));
    }


    private void OnSceneLoaded(SceneTypes scene)
    {
        NetworkUtility.SendMessage<SyncStateSceneMessage>(new());
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
        NetworkServer.RegisterHandler<SyncStateSceneMessage>(OnSceneMoveSync);
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

        MoveClientObjectsToScene(client, message.sceneType);

        client.Send(new SceneChangeRequestMessage(message.sceneType));
    }

    private void OnSceneMoveSync(NetworkConnectionToClient client, SyncStateSceneMessage message)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(client, out var sceneType))
        {
            MoveClientObjectsToScene(client, sceneType);
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

        if (ConnectionInfo.ClientEntities.TryGetValue(client, out var entities))
        {
            foreach (var entityId in entities)
            {
                if (NetworkServer.spawned.TryGetValue(entityId, out var networkIdentity))
                {
                    SceneManager.MoveGameObjectToScene(networkIdentity.gameObject, scene);
                }
            }
        }
    }
}
