using System;
using System.Collections.Generic;
using BitterECS.Core;
using BitterECS.Core.Integration;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private IEnumerable<IProviderHandler> _providers;

    [Inject]
    public EntryPointClient(
        NetworkConfig clientConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers)
    {
        _networkConfig = clientConfig;
        _networkManager = networkManager;
        _providers = providers;
    }

    public void Start()
    {
        LoggerUtility.Info("[Client] Injecting client...");
        _networkConfig.Configure(_networkManager);
        SceneLoader.LoadScene(SceneTypes.Menu);
    }

    public void SetupConnection()
    {
        _networkManager.StartClient();
        SetupProvider();
        OnSubscribeClient();
        OnClientStart();
    }

    private void SetupProvider()
    {
        foreach (var provider in _providers)
        {
            provider.HandlersClient();
        }
    }

    private void OnClientStart()
    {
        LoggerUtility.Info("[Client] Client started!");
        EcsSystems.Run<IClientStart>(system => system.Start());
    }

    private void OnClientConnected()
    {
        LoggerUtility.Info("[Client] Client connected successfully!");
        EcsSystems.Run<IClientConnected>(system => system.Connect());
    }

    private void OnClientDisconnected()
    {
        LoggerUtility.Error("[Client] Connection failed or disconnected!");
        EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
    }

    private void OnClientError(TransportError error, string arg2)
    {
        LoggerUtility.Error("[Client] Connection failed or disconnected!");
        EcsSystems.Run<IClientError>(system => system.OnError());
    }

    private void OnSubscribeClient()
    {
        LoggerUtility.Info("[Client] Subscribing to events...");
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnErrorEvent += OnClientError;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
    }

    private void OnUnsubscribeClient()
    {
        LoggerUtility.Info("[Client] Unsubscribing from events...");
        NetworkClient.OnConnectedEvent -= OnClientConnected;
        NetworkClient.OnErrorEvent -= OnClientError;
        NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
    }

    public void Dispose()
    {
        if (NetworkClient.active)
        {
            OnUnsubscribeClient();
        }
    }

}
