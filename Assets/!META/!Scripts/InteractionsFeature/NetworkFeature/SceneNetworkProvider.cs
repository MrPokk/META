using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkProvider : IHandlerMessages
{
    private SceneConfig _sceneConfig;
    private Dictionary<NetworkConnection, SceneTypes> _clientSceneTypes;

    [Inject]
    public SceneNetworkProvider(SceneConfig sceneConfig)
    {
        _sceneConfig = sceneConfig;
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
        var sceneFromClient = _sceneConfig.GetServerLoadScenes();
        if (!sceneFromClient.Any())
        {
            Debug.LogError($"No scenes to load to server");
            return;
        }

        foreach (var additiveScene in _sceneConfig.GetServerLoadScenes())
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
            Debug.LogError("NetworkServer is not active");
            return;
        }
        if (!ValidateScene(sceneType))
        {
            Debug.LogError($"Invalid scene type: {sceneType}");
            return;
        }

        _clientSceneTypes[client] = sceneType;
        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(client, () => client.Send(new SceneChangeRequestMessage(sceneType))));
    }

    [Server]
    private bool ValidateScene(SceneTypes sceneType)
    {
        if (sceneType == SceneTypes.None) return false;

        foreach (var mapping in _sceneConfig.sceneMappings)
        {
            if (mapping.sceneType == sceneType && mapping.isLoadServer)
            {
                return true;
            }
        }
        Debug.LogError($"Invalid scene type: {sceneType}");
        return false;
    }

    public bool TryGetCurrentSceneToClient(NetworkConnection connection, out (string sceneName, SceneTypes sceneType) valueScene)
    {
        if (!_clientSceneTypes.TryGetValue(connection, out var sceneType))
        {
            Debug.LogWarning("No scene type found for client");
            valueScene = default;
            return false;
        }

        valueScene = (_sceneConfig.GetSceneName(sceneType), sceneType);
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
        if (!NetworkClient.active) { Debug.LogError("NetworkClient is not active"); return; }
        SceneLoader.LoadScene(message.sceneType);
    }

    #endregion
}
