using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkManager : NetworkBehaviour, IServerConnected, IServerDisconnected
{
    [Inject]
    private SceneConfig _sceneConfig;

    private Dictionary<NetworkConnection, SceneTypes> _clientSceneTypes;

    public Priority PrioritySystem => Priority.FIRST_TASK;

    #region Server Methods

    override public void OnStartServer()
    {
        base.OnStartServer();
        SetupLoadServerScene();
    }

    [Server]
    private async void SetupLoadServerScene()
    {
        _clientSceneTypes = new();
        foreach (var additiveScene in _sceneConfig.GetServerLoadScenes())
        {
            await SceneLoader.LoadSceneAsync(additiveScene.sceneType, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
            });
        }
    }

    [Server]
    public void ChangeClientScene(NetworkConnectionToClient target, SceneTypes sceneType)
    {
        if (!ValidateScene(sceneType))
            return;

        _clientSceneTypes[target] = sceneType;
        TargetLoadScene(target, sceneType);
    }

    [Server]
    private void InitializeClientScene(NetworkConnectionToClient target)
    {
        var initialScene = _sceneConfig.firstSceneToLoadClient;
        _clientSceneTypes[target] = initialScene;
        TargetLoadScene(target, initialScene);
    }

    [Server]
    private bool ValidateScene(SceneTypes sceneType)
    {
        if (sceneType == SceneTypes.None)
            return false;

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
    #endregion

    #region Client Methods
    [Command]
    public void CmdRequestSceneChange(SceneTypes sceneType, NetworkConnectionToClient target = null)
    {
        ChangeClientScene(target, sceneType);
    }

    [TargetRpc]
    private void TargetLoadScene(NetworkConnectionToClient target, SceneTypes sceneType)
    {
        SceneLoader.LoadScene(sceneType);
    }

    #endregion

    #region Connection Handling

    public void Connect(NetworkConnectionToClient client)
    {
        if (!NetworkServer.active) return;
        InitializeClientScene(client);
    }

    public void Disconnect(NetworkConnectionToClient client)
    {
        if (!NetworkServer.active) return;
        _clientSceneTypes.Remove(client);
    }
    #endregion
}
