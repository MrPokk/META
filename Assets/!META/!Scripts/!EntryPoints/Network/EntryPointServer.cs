using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointServer : IStartable, IDisposable
{
    private readonly NetworkConfig _networkConfig;
    private readonly NetworkManager _networkManager;
    private readonly IEnumerable<IHandlerMessages> _handlerMessages;


    [Inject]
    public EntryPointServer(
        NetworkConfig networkConfig,
        NetworkManager networkManager,
        IEnumerable<IHandlerMessages> handlerMessages)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
        _handlerMessages = handlerMessages;
    }

    public void Start()
    {
        LoggerUtility.Info("[Server] Injecting server...");
        _networkConfig.Configure(_networkManager);
        _networkManager.StartServer();
        NetworkUtility.SetupHandlers(_handlerMessages);
        SubscribeServerEvents();
        OnServerStart();
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
