using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class EntryPointServer : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private readonly IEnumerable<IProviderHandler> _providers;

    [Inject]
    public EntryPointServer(
        NetworkConfig networkConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
        _providers = providers;
    }

    public void Start()
    {
        LoggerUtility.Info("[Server] Injecting server...");
        _networkConfig.Configure(_networkManager);
        _networkManager.StartServer();
        SetupProvider();
        SubscribeServerEvents();
        OnServerStart();
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
        LoggerUtility.Info("[Server] Server started!");
        EcsSystems.Run<IServerStart>(system => system.Start());
    }

    private void OnServerConnected(NetworkConnectionToClient client)
    {
        LoggerUtility.Info("[Server] Server connected!");
        EcsSystems.Run<IServerConnected>(system => system.Connect(client));
    }

    private void OnServerError(NetworkConnectionToClient client, TransportError error, string arg3)
    {
        LoggerUtility.Info($"[Server] Server error: {error}");
        EcsSystems.Run<IServerError>(system => system.OnError(client, error, arg3));
    }

    private void OnServerDisconnected(NetworkConnectionToClient client)
    {
        LoggerUtility.Info("[Server] Server disconnected!");
        EcsSystems.Run<IServerDisconnected>(system => system.Disconnect(client));
    }

    private void SubscribeServerEvents()
    {
        LoggerUtility.Info("[Server] Subscribing to events...");
        NetworkServer.OnConnectedEvent += OnServerConnected;
        NetworkServer.OnDisconnectedEvent += OnServerDisconnected;
        NetworkServer.OnErrorEvent += OnServerError;
    }

    private void UnsubscribeServerEvents()
    {
        LoggerUtility.Info("[Server] Unsubscribing from events...");
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
