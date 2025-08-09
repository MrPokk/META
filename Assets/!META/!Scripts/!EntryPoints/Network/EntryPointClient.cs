using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkObjectPrefabConfig _networkObjectPrefabConfig;
    private readonly NetworkManager _networkManager;
    private readonly IEnumerable<IHandlerMessages> _handlerMessages;

    [Inject]
    public EntryPointClient(
        NetworkConfig clientConfig,
        NetworkObjectPrefabConfig networkObjectPrefabConfig,
        NetworkManager networkManager,
        IEnumerable<IHandlerMessages> handlerMessages)
    {
        _networkConfig = clientConfig;
        _networkObjectPrefabConfig = networkObjectPrefabConfig;
        _networkManager = networkManager;
        _handlerMessages = handlerMessages;
    }

    public void Start()
    {
        Debug.Log("[Client] Injecting client...");
        _networkConfig.Configure(_networkManager);
        SceneLoader.LoadScene(SceneTypes.Menu);
    }

    public void SetupConnection()
    {
        _networkManager.StartClient();
        _networkObjectPrefabConfig.RegisterAllPrefabs();
        NetworkUtility.SetupHandlers(_handlerMessages);
        OnSubscribeClient();
        OnClientStart();
    }

    private void OnClientStart()
    {
        Debug.Log("[Client] Client started!");
        EcsSystems.Run<IClientStart>(system => system.Start());
    }

    private void OnClientConnected()
    {
        Debug.Log("[Client] Client connected successfully!");
        EcsSystems.Run<IClientConnected>(system => system.Connect());
    }

    private void OnClientDisconnected()
    {
        Debug.LogError("[Client] Connection failed or disconnected!");
        EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
    }

    private void OnClientError(TransportError error, string arg2)
    {
        Debug.LogError("[Client] Connection failed or disconnected!");
        EcsSystems.Run<IClientError>(system => system.OnError());
    }

    private void OnSubscribeClient()
    {
        Debug.Log("[Client] Subscribing to events...");
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnErrorEvent += OnClientError;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
    }

    private void OnUnsubscribeClient()
    {
        Debug.Log("[Client] Unsubscribing from events...");
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
