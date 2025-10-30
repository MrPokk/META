using Mirror;
using UnityEngine.SceneManagement;

public class SceneNetworkProvider : IProviderHandler
{
    public static void ChangeScene(SceneTypes sceneType) => NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);
    }

    private void OnClientRequest(SceneChangeRequestMessage message)
    {
        SceneLoader.LoadScene(message.sceneType);
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
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

        MoveClientObjectsToScene(client, message.sceneType);
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
                    NetworkServer.RemovePlayerForConnection(client, RemovePlayerOptions.Unspawn);
                    SceneManager.MoveGameObjectToScene(networkIdentity.gameObject, scene);
                    NetworkServer.AddPlayerForConnection(client, networkIdentity.gameObject);
                }
            }
        }
    }
}
