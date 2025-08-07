using System;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly OverrideNetworkManager _networkManager;

    public Priority PrioritySystem => Priority.FIRST_TASK;

    public EntryPointClient() { }
    [Inject]
    public EntryPointClient(NetworkConfig clientConfig, OverrideNetworkManager networkManager)
    {
        _networkConfig = clientConfig;
        _networkManager = networkManager;
    }

    public void Start()
    {
        _networkConfig.Configure(_networkManager);
    }    

    public void InitializeClient()
    {
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
        Debug.LogError("<>[Network] Connection failed or disconnected!");
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
