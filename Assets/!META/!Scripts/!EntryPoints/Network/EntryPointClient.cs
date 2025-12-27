using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
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
        _networkConfig.Configure(_networkManager);
        SceneLoader.LoadScene(SceneTypes.Menu);
    }

    public void SetupConnection()
    {
        LoggerUtility.Info("Injecting client...", NetworkType.Client);
        _networkManager.StartClient();
        SetupProvider();
        OnSubscribeClient();
        OnClientStart();
        LoggerUtility.Info("Client injected successfully!", NetworkType.Client);
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
        EcsSystems.Run<IClientStart>(system => system.Start());
    }

    private void OnClientConnected()
    {
        EcsSystems.Run<IClientConnected>(system => system.Connect());
    }

    private void OnClientDisconnected()
    {
        EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
    }

    private void OnClientError(TransportError error, string arg2)
    {
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
