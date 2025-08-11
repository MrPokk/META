using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkProvider : IProviderHandler
{
    public SceneConfig SceneConfig { get; }

    [Inject]
    public SceneNetworkProvider(SceneConfig sceneConfig)
    {
        SceneConfig = sceneConfig;
    }

    #region Server Implementation

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerChangeScene);
        SetupLoadServerScene();
    }

    [Server]
    public void SetupLoadServerScene()
    {
        var serverScenes = SceneConfig.GetServerLoadScenes();

        foreach (var scene in serverScenes)
        {
            SceneLoader.LoadScene(scene.sceneType, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
        }
    }

    [Server]
    private void OnServerChangeScene(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        if (!NetworkUtility.IsServerActive())
            return;

        var sceneType = message.sceneType;

        if (!SceneConfig.ValidateScene(sceneType, mapping => mapping.isLoadServer))
            return;

        UpdateClientScene(client, sceneType);
        NotifyClientAboutSceneChange(client, sceneType);
    }

    [Server]
    private void UpdateClientScene(NetworkConnectionToClient client, SceneTypes newSceneType)
    {
        if (ConnectionInfo.ClientSceneTypes.TryGetValue(client, out var previousScene))
        {
            ConnectionInfo.SceneToConnections[previousScene]?.Remove(client);
        }

        ConnectionInfo.ClientSceneTypes[client] = newSceneType;

        if (!ConnectionInfo.SceneToConnections.TryGetValue(newSceneType, out var connections))
        {
            connections = new HashSet<NetworkConnectionToClient>();
            ConnectionInfo.SceneToConnections[newSceneType] = connections;
        }

        connections.Add(client);
    }

    [Server]
    private void NotifyClientAboutSceneChange(NetworkConnection client, SceneTypes sceneType)
    {
        client.Send(new SceneChangeRequestMessage(sceneType));
    }

    [Server]
    public SceneTypes GetCurrentTypeSceneToClient(NetworkConnectionToClient connection)
    {
        if (ConnectionInfo.ClientSceneTypes.TryGetValue(connection, out var sceneType))
            return sceneType;

        LoggerUtility.Warning($"No scene type found for client {connection}");
        return SceneTypes.None;
    }

    [Server]
    public bool TryGetCurrentSceneToClient(
        NetworkConnectionToClient connection,
        out (Scene sceneObject, SceneTypes sceneType) sceneInfo)
    {
        if (!ConnectionInfo.ClientSceneTypes.TryGetValue(connection, out var sceneType))
        {
            LoggerUtility.Warning($"No scene type found for client {connection}");
            sceneInfo = default;
            return false;
        }

        var sceneName = SceneConfig.GetSceneName(sceneType);
        sceneInfo = (SceneManager.GetSceneByName(sceneName), sceneType);
        return true;
    }

    [Server]
    public IReadOnlyCollection<NetworkConnectionToClient> GetConnectionsOnScene(SceneTypes sceneType)
    {
        return (ConnectionInfo.SceneToConnections.GetValueOrDefault(sceneType) ?? new HashSet<NetworkConnectionToClient>());
    }

    [Server]
    public int GetNetworkCountOnScene(SceneTypes sceneType)
    {
        return ConnectionInfo.ClientSceneTypes.Count(kvp => kvp.Value == sceneType);
    }

    [Server]
    private void OnClientDisconnected(NetworkConnectionToClient connection)
    {
        if (ConnectionInfo.ClientSceneTypes.TryGetValue(connection, out var sceneType))
        {
            ConnectionInfo.SceneToConnections[sceneType]?.Remove(connection);
            ConnectionInfo.ClientSceneTypes.Remove(connection);
        }
    }

    #endregion

    #region Client Implementation

    [Client]
    public static void ClientChangeScene(SceneTypes sceneType)
    {
        CoroutineUtility.Run(
        NetworkUtility.WaitingToConnect(
        NetworkClient.connection, () => NetworkClient.Send(new SceneChangeRequestMessage(sceneType))));
    }

    [Client]
    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientChangeScene);
    }

    [Client]
    private static void OnClientChangeScene(SceneChangeRequestMessage message)
    {
        if (!NetworkUtility.IsClientActive())
            return;

        SceneLoader.LoadScene(message.sceneType);
    }

    #endregion
}
