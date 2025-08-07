using System.Collections;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkManager : NetworkBehaviour, IServerConnected, IServerStart, IServerDisconnected
{
    [Inject]
    private SceneConfig _sceneConfig;

    private readonly Dictionary<NetworkConnection, SceneTypes> _clientSceneTypes = new();

    public Priority PrioritySystem => throw new System.NotImplementedException();

    #region Server Methods

    public void Start()
    {
        SetupLoadServerScene();
    }

    [Server]
    private async void SetupLoadServerScene()
    {
        foreach (var additiveScene in _sceneConfig.GetServerLoadScenes())
        {
            await SceneLoader.LoadSceneAsync(additiveScene.sceneType, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
            });
        }
    }

    [Server]
    public void ChangeClientScene(NetworkConnectionToClient conn, SceneTypes sceneType)
    {
        if (!ValidateScene(sceneType)) return;

        _clientSceneTypes[conn] = sceneType;
        string sceneName = _sceneConfig.GetSceneName(sceneType);
        TargetLoadScene(conn, sceneName);
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
    public void CmdRequestSceneChange(SceneTypes sceneType, NetworkConnectionToClient conn = null)
    {
        ChangeClientScene(conn, sceneType);
    }

    [TargetRpc]
    private void TargetLoadScene(NetworkConnection conn, string sceneName)
    {
        CoroutineUtility.Run(LoadClientSceneAsync(sceneName));
    }

    private IEnumerator LoadClientSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnClientSceneLoaded(sceneName);
    }

    private void OnClientSceneLoaded(string sceneName)
    {
        Debug.Log($"Client loaded scene: {sceneName}");
    }
    #endregion

    #region Connection Handling


    public void Connect(NetworkConnectionToClient client)
    {
        InitializeClientScene(client);
    }

    private void InitializeClientScene(NetworkConnection conn)
    {
        SceneTypes initialScene = _sceneConfig.firstSceneToLoadClient;
        _clientSceneTypes[conn] = initialScene;
        string sceneName = _sceneConfig.GetSceneName(initialScene);
        TargetLoadScene(conn, sceneName);
    }
    public void Disconnect(NetworkConnectionToClient client)
    {
        _clientSceneTypes.Remove(client);
    }
    #endregion

    #region Helper Methods
    [Server]
    public SceneTypes GetClientSceneType(NetworkConnection conn)
    {
        return _clientSceneTypes.TryGetValue(conn, out var sceneType) ? sceneType : SceneTypes.None;
    }

    [Server]
    public string GetClientSceneName(NetworkConnection conn)
    {
        return _clientSceneTypes.TryGetValue(conn, out var sceneType)
            ? _sceneConfig.GetSceneName(sceneType)
            : string.Empty;
    }



    #endregion
}


