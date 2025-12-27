using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

public class EntryPointServer : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private readonly IEnumerable<IProviderHandler> _providers;
    private readonly SceneConfig _sceneConfig;

    [Inject]
    public EntryPointServer(
        NetworkConfig networkConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers,
        SceneConfig sceneConfig)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
        _providers = providers;
        _sceneConfig = sceneConfig;
    }

    public void Start()
    {
        LoggerUtility.Info("Injecting server...", NetworkType.Server);
        _networkConfig.Configure(_networkManager);
        _networkManager.StartServer();
        SetupServerScenes();
        SetupProvider();
        SubscribeServerEvents();
        OnServerStart();
        LoggerUtility.Info("Server started successfully!", NetworkType.Server);
    }

    private void SetupServerScenes()
    {
        var serverScenes = _sceneConfig.GetServerLoadScenes();
        foreach (var scene in serverScenes)
        {
            SceneLoader.LoadScene(scene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
        }
    }

    private void SetupProvider()
    {
        foreach (var provider in _providers)
        {
            provider.HandlersServer();
        }
    }

    private void OnServerStart()
    {
        EcsSystems.Run<IServerStart>(system => system.Start());
    }

    private void OnServerConnected(NetworkConnectionToClient client)
    {
        EcsSystems.Run<IServerConnected>(system => system.Connect(client));
    }

    private void OnServerError(NetworkConnectionToClient client, TransportError error, string arg3)
    {
        EcsSystems.Run<IServerError>(system => system.OnError(client, error, arg3));
    }

    private void OnServerDisconnected(NetworkConnectionToClient client)
    {
        EcsSystems.Run<IServerDisconnected>(system => system.Disconnect(client));
    }

    private void SubscribeServerEvents()
    {
        NetworkServer.OnConnectedEvent += OnServerConnected;
        NetworkServer.OnDisconnectedEvent += OnServerDisconnected;
        NetworkServer.OnErrorEvent += OnServerError;
    }

    private void UnsubscribeServerEvents()
    {
        NetworkServer.OnConnectedEvent -= OnServerConnected;
        NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;
        NetworkServer.OnErrorEvent -= OnServerError;
    }

    public void Dispose()
    {
        if (NetworkServer.active)
        {
            UnsubscribeServerEvents();
        }
    }
}
