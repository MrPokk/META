using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable, IDisposable
{
    private static NetworkManager s_networkManager;
    private static NetworkConfig s_networkConfig;
    private static IEnumerable<IProviderHandler> s_providers;
    public static uint ClientID => NetworkClient.connection?.identity?.netId ?? 0;

    [Inject]
    public EntryPointClient(
        NetworkConfig clientConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers)
    {
        s_networkConfig = clientConfig;
        s_networkManager = networkManager;
        s_providers = providers;
    }

    public void Start()
    {
        s_networkConfig.Configure(s_networkManager);
        SceneLoader.LoadScene(SceneTypes.Menu);
    }

    public static void SetupConnection()
    {
        if (NetworkUtility.IsClientReady())
        {
            LoggerUtility.Info("Client already started", NetworkType.Client);
            return;
        }

        LoggerUtility.Info("Starting client...", NetworkType.Client);

        s_networkManager.StartClient();
        SetupProvider();
        OnSubscribeClient();
    }

    private static void SetupProvider()
    {
        foreach (var provider in s_providers)
        {
            provider.HandlersClient();
        }
    }

    private static void OnClientConnected()
    {
        EcsSystems.Run<IClientConnected>(system => system.Connect());
    }

    private static void OnClientDisconnected()
    {
        EcsSystems.Run<IClientDisconnected>(system => system.Disconnect());
    }

    private static void OnClientError(TransportError error, string arg2)
    {
        EcsSystems.Run<IClientError>(system => system.OnError());
    }

    private static void OnSubscribeClient()
    {
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnErrorEvent += OnClientError;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;
    }

    private static void OnUnsubscribeClient()
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
