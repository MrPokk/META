using System;
using System.Collections;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneNetworkProvider : IServerStart
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public static SceneNetworkProvider Instance { get; private set; }
    private readonly SceneConfig _sceneConfig;
    private readonly Dictionary<NetworkConnection, SceneTypes> _clientSceneTypes;

    public SceneNetworkProvider() { }

    [Inject]
    public SceneNetworkProvider(SceneConfig sceneConfig)
    {
        Instance = this;
        _sceneConfig = sceneConfig;
        _clientSceneTypes = new();
        SetupLoadServerScene();
    }

    public void Start()
    {
        RegisterMessageHandlers();
    }

    #region Server Methods

    private void SetupLoadServerScene()
    {
        if (!NetworkServer.active) return;

        foreach (var additiveScene in _sceneConfig.GetServerLoadScenes())
        {
            SceneLoader.LoadScene(additiveScene.sceneType, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
            });
        }
    }

    private void ChangeClientScene(NetworkConnectionToClient target, SceneTypes sceneType)
    {
        if (!NetworkServer.active) return;
        if (!ValidateScene(sceneType)) return;

        _clientSceneTypes[target] = sceneType;
        CoroutineUtility.Run(WaitingClientToConnect(target, () => target.Send(new SceneChangeRequestMessage { sceneType = sceneType })));
    }

    private IEnumerator WaitingClientToConnect(NetworkConnectionToClient target, Action callback)
    {
        if (target == null)
        {
            Debug.LogError("WaitingClientToConnect: target is null");
            yield break;
        }

        yield return new WaitUntil(() => target.isReady);
        callback?.Invoke();
    }

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


    #endregion

    #region Client Methods

    public void InitializeClientScene(NetworkConnectionToClient target)
    {
        if (!NetworkServer.active) { Debug.LogError("RegisterMessageHandlers: NetworkServer is not active"); return; }

        var initialScene = _sceneConfig.firstSceneToLoadClient;
        _clientSceneTypes[target] = initialScene;
        // CoroutineUtility.Run(WaitingClientToConnect(target, () =>target.Send(new SceneChangeRequestMessage { sceneType = initialScene })));
    }

    public void RequestSceneChange(SceneTypes sceneType)
    {
        if (!NetworkServer.active) { Debug.LogError("RegisterMessageHandlers: NetworkServer is not active"); return; }

        NetworkClient.Send(new SceneChangeRequestMessage { sceneType = sceneType });
    }

    public void RemoveClientScene(NetworkConnectionToClient target)
    {
        if (target != null && _clientSceneTypes.ContainsKey(target))
        {
            _clientSceneTypes.Remove(target);
        }
    }

    #endregion

    #region Message Handlers

    private void RegisterMessageHandlers()
    {
        if (!NetworkServer.active) { Debug.LogError("RegisterMessageHandlers: NetworkServer is not active"); return; }

        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnSceneChangeRequested);
    }

    private void OnSceneChangeRequested(NetworkConnection conn, SceneChangeRequestMessage msg)
    {
        ChangeClientScene(conn as NetworkConnectionToClient, msg.sceneType);
    }

    #endregion
}
