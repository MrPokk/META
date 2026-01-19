using System;
using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

public class EntryPointServer : IStartable, IDisposable
{
    private static NetworkConfig s_networkConfig;
    private static NetworkManager s_networkManager;
    private static IEnumerable<IProviderHandler> s_providers;
    private static SceneConfig s_sceneConfig;

    [Inject]
    public EntryPointServer(
        NetworkConfig networkConfig,
        NetworkManager networkManager,
        IEnumerable<IProviderHandler> providers,
        SceneConfig sceneConfig)
    {
        s_networkConfig = networkConfig;
        s_networkManager = networkManager;
        s_providers = providers;
        s_sceneConfig = sceneConfig;
    }

    public void Start()
    {
        LoggerUtility.Info("Starting server...", NetworkType.Server);
        s_networkConfig.Configure(s_networkManager);
        s_networkManager.StartServer();
        SetupServerScenes();
        SetupNotGraphicServer();
        SetupProvider();
        SubscribeServerEvents();
        OnServerStart();
    }

    private static void SetupNotGraphicServer()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        Screen.SetResolution(850, 500, false);

        foreach (var cam in Camera.allCameras)
        {
            cam.enabled = false;
            cam.gameObject.SetActive(false);
        }

        LightmapSettings.lightmaps = new LightmapData[0];
        RenderSettings.ambientLight = Color.black;
        RenderSettings.fog = false;
        DynamicGI.UpdateEnvironment();
    }

    private static void SetupServerScenes()
    {
        var serverScenes = s_sceneConfig.GetServerLoadScenes();
        foreach (var scene in serverScenes)
        {
            SceneLoader.LoadScene(scene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
        }
    }

    private static void SetupProvider()
    {
        foreach (var provider in s_providers)
        {
            provider.HandlersServer();
        }
    }

    private static void OnServerStart()
    {
        EcsSystems.Run<IServerStart>(system => system.Start());
    }

    private static void OnServerConnected(NetworkConnectionToClient client)
    {
        EcsSystems.Run<IServerConnected>(system => system.Connect(client));
    }

    private static void OnServerError(NetworkConnectionToClient client, TransportError error, string arg3)
    {
        EcsSystems.Run<IServerError>(system => system.OnError(client, error, arg3));
    }

    private static void OnServerDisconnected(NetworkConnectionToClient client)
    {
        EcsSystems.Run<IServerDisconnected>(system => system.Disconnect(client));
    }

    private static void SubscribeServerEvents()
    {
        NetworkServer.OnConnectedEvent += OnServerConnected;
        NetworkServer.OnDisconnectedEvent += OnServerDisconnected;
        NetworkServer.OnErrorEvent += OnServerError;
    }

    private static void UnsubscribeServerEvents()
    {
        NetworkServer.OnConnectedEvent -= OnServerConnected;
        NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;
        NetworkServer.OnErrorEvent -= OnServerError;
    }

    public void Dispose()
    {
        UnsubscribeServerEvents();
        LoggerUtility.Info("Server stop successfully!", NetworkType.Server);
    }
}
