using System.Collections.Generic;
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
        NetworkUtility.SendMessage<SyncStateSceneMessage>(new());
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
    }

    private void OnServerRequest(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        if (!SceneConfig.IsServerScene(message.sceneType))
            LoggerUtility.Error($"Scene {message.sceneType} is not a server scene!");

        ConnectionInfo.ClientToScene[client] = message.sceneType;
        ConnectionInfo.SceneToConnections.GetOrAdd(message.sceneType, _ => new() { client }).Add(client);

        client.Send(new SceneChangeRequestMessage(message.sceneType));
    }
}
