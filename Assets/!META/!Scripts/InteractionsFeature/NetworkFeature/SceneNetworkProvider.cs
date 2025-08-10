using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkProvider : IHandlerMessages
{
    public SceneConfig SceneConfig { get; private set; }
    private Dictionary<NetworkConnection, SceneTypes> _clientSceneTypes;

    [Inject]
    public SceneNetworkProvider(SceneConfig sceneConfig)
    {
        SceneConfig = sceneConfig;
        _clientSceneTypes = new();
    }

    #region Server

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerChangeScene);
        SetupLoadServerScene();
    }

    [Server]
    public void SetupLoadServerScene()
    {
        var sceneFromClient = SceneConfig.GetServerLoadScenes();
        if (!sceneFromClient.Any())
        {
            LoggerUtility.Error($"No scenes to load to server");
            return;
        }

        foreach (var additiveScene in SceneConfig.GetServerLoadScenes())
        {
            SceneLoader.LoadScene(additiveScene.sceneType, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
            });
        }
    }

    [Server]
    public void OnServerChangeScene(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        var sceneType = message.sceneType;
        if (!NetworkServer.active)
        {
            LoggerUtility.Error("NetworkServer is not active");
            return;
        }
        if (!ValidateScene(sceneType))
        {
            LoggerUtility.Error($"Invalid scene type: {sceneType}");
            return;
        }

        _clientSceneTypes[client] = sceneType;
        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(client, () => client.Send(new SceneChangeRequestMessage(sceneType))));
    }

    [Server]
    private bool ValidateScene(SceneTypes sceneType)
    {
        if (sceneType == SceneTypes.None) return false;

        foreach (var mapping in SceneConfig.sceneMappings)
        {
            if (mapping.sceneType == sceneType && mapping.isLoadServer)
            {
                return true;
            }
        }
        LoggerUtility.Error($"Invalid scene type: {sceneType}");
        return false;
    }

    public bool TryGetCurrentSceneToClient(NetworkConnection connection, out (Scene sceneObject, SceneTypes sceneType) valueScene)
    {
        if (!_clientSceneTypes.TryGetValue(connection, out var sceneType))
        {
            LoggerUtility.Warning("No scene type found for client");
            valueScene = default;
            return false;
        }

        var sceneName = SceneConfig.GetSceneName(sceneType);
        valueScene = (SceneManager.GetSceneByName(sceneName), sceneType);
        return true;
    }

    #endregion

    #region Client

    [Client]
    public static void ClientChangeScene(SceneChangeRequestMessage message)
    {
        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(NetworkClient.connection, () => OnMessageClientChangeScene(message)));
    }

    [Client]
    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientChangeScene);
    }

    private static void OnMessageClientChangeScene(SceneChangeRequestMessage message)
    {
        NetworkClient.Send(message);
    }

    [Client]
    private static void OnClientChangeScene(SceneChangeRequestMessage message)
    {
        if (!NetworkClient.active) { LoggerUtility.Error("NetworkClient is not active"); return; }
        SceneLoader.LoadScene(message.sceneType);
    }

    #endregion
}
