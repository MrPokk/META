using System;
using BitterECS.Core;
using kcp2k;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointServer : IStartable, IDisposable
{
    private readonly NetworkServerConfig _networkConfig;
    private readonly NetworkManager _networkManager;

    [Inject]
    public EntryPointServer(NetworkServerConfig networkConfig, NetworkManager networkManager)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
    }

    public void Start()
    {
        InitializeServer();
    }

    private void InitializeServer()
    {
        _networkConfig.ConfigureServer(_networkManager);
        SetupTransports();

        _networkManager.StartServer();
        SubscribeServerEvents();
    }

    private void SetupTransports()
    {
#if UNITY_WEBGL
        _networkManager.transport = _networkManager.GetComponent<SimpleWebTransport>();
        Debug.Log("[Network] Using WebSockets for WebGL");
#else
        _networkManager.transport = _networkManager.GetComponent<KcpTransport>();
        Debug.Log("[Network] Using KCP for Desktop/Server");
#endif
    }

    private void OnServerConnected(NetworkConnectionToClient client)
    {
        Debug.Log("[Network] Server connected!");
        EcsSystems.Run<IServerConnected>(system => system.Connect());
    }

    private void OnServerError(NetworkConnectionToClient client, TransportError error, string arg3)
    {
        Debug.Log($"[Network] Server error: {error}");
        EcsSystems.Run<IServerError>(system => system.OnError());
    }

    private void OnServerDisconnected(NetworkConnectionToClient client)
    {
        Debug.Log("[Network] Server disconnected!");
        EcsSystems.Run<IServerDisconnected>(system => system.Disconnect());
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
