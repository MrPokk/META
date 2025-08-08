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
    private readonly SceneNetworkProvider _sceneNetworkProvider;

    public Priority PrioritySystem => Priority.FIRST_TASK;

    [Inject]
    public EntryPointClient(NetworkConfig clientConfig, OverrideNetworkManager networkManager, SceneNetworkProvider sceneNetworkProvider)
    {
        _networkConfig = clientConfig;
        _networkManager = networkManager;
        _sceneNetworkProvider = sceneNetworkProvider;
    }

    public void Start()
    {
        Debug.Log("[Client] Starting client...");
        _networkConfig.Configure(_networkManager);
        SceneLoader.LoadScene(SceneTypes.Menu);
    }

    public void SetupConnection()
    {
        _networkManager.StartClient();
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
