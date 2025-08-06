using System;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private readonly NetworkClientConfig _clientConfig;
    private readonly NetworkManager _networkManager;

    [Inject]
    public EntryPointClient(NetworkClientConfig clientConfig, NetworkManager networkManager)
    {
        _clientConfig = clientConfig;
        _networkManager = networkManager;
    }

    public void Start()
    {
#if UNITY_WEBGL
        _networkManager.networkAddress = _clientConfig.webSocketServerUrl;
#else
        _networkManager.networkAddress = _clientConfig.serverIP;
#endif
        _networkManager.StartClient();
        OnSubscribeClient();
    }
    
    private void OnClientConnected()
    {
        Debug.Log("[Network] Client connected successfully!");
        EcsSystems.Run<IClientConnected>(system => system.Connect());
    }

    private void OnClientDisconnected()
    {
        Debug.LogError("[Network] Connection failed or disconnected!");
        EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
    }

    private void OnClientError(TransportError error, string arg2)
    {
        Debug.LogError("[Network] Connection failed or disconnected!");
        EcsSystems.Run<IClientError>(system => system.OnError());
    }

    private void OnSubscribeClient()
    {
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnErrorEvent += OnClientError;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
    }

    private void OnUnsubscribeClient()
    {
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
