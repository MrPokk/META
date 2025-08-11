using System;
using System.Linq;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using BitterECS.Utility;


#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EntryPointProject : LifetimeScope
{
    [Header("<size=18>Configs</size>")]
    [SerializeField] private LoggerConfig LoggerConfig;
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        RegisterSharedDependencies(builder);

#if UNITY_EDITOR
        SetupEditorMode(builder);
#else
        SetupBuildMode(builder);
#endif
    }

    #region Registration

    private void RegisterSharedDependencies(IContainerBuilder builder)
    {
        RegisterLogger();
        RegisterConfigs(builder);
        RegisterSceneLoader(builder);
        RegisterProvider(builder);
        RegisterNetworkManager(builder);
        RegisterEcsManager(builder);
    }

    private void RegisterConfigs(IContainerBuilder builder)
    {
        builder.RegisterInstance(_networkConfig);
        builder.RegisterInstance(_sceneConfig);
    }

    private void RegisterLogger()
    {
        LoggerUtility.Initialize(LoggerConfig);
    }

    private void RegisterSceneLoader(IContainerBuilder builder)
    {
        var sceneLoader = CreateSceneLoader();
        builder.RegisterInstance(sceneLoader);
    }

    private void RegisterProvider(IContainerBuilder builder)
    {
        var providerTypes = ReflectionUtility.FindAllAssignments<IProviderHandler>();

        foreach (var type in providerTypes)
        {
            builder.Register(type, Lifetime.Singleton).As(type).AsImplementedInterfaces();
        }

        builder.Register<ConnectionInfo>(Lifetime.Singleton);
    }

    private void RegisterNetworkManager(IContainerBuilder builder)
    {
        var networkManager = CreateNetworkManager();
        builder.RegisterComponent(networkManager)
               .As<NetworkManager>()
               .AsImplementedInterfaces();
    }

    private void RegisterEcsManager(IContainerBuilder builder)
    {
        var ecsManager = CreateEcsManager();
        builder.RegisterComponent(ecsManager)
               .As<EcsNetworkUnity>()
               .AsImplementedInterfaces();
    }

    #endregion

    #region Components

    private SceneLoader CreateSceneLoader()
    {
        var loader = SceneLoader.Initialize(_sceneConfig);
        SceneLoader.LoadScene(SceneTypes.EntryPoint);
        return loader;
    }

    private EcsNetworkUnity CreateEcsManager()
    {
        var ecsManager = new GameObject("[EcsManager]", typeof(EcsNetworkUnity))
            .GetComponent<EcsNetworkUnity>();
        DontDestroyOnLoad(ecsManager.gameObject);
        return ecsManager;
    }

    private NetworkManager CreateNetworkManager()
    {
        var manager = new GameObject("[NetworkManager]",
                typeof(KcpTransport),
                typeof(SimpleWebTransport),
                typeof(SceneInterestManagement),
                typeof(NetworkManager))
            .GetComponent<NetworkManager>();

        SetupTransportForPlatform(manager);
        DontDestroyOnLoad(manager.gameObject);
        return manager;
    }

    private void SetupTransportForPlatform(NetworkManager manager)
    {
        manager.transport = Application.platform == RuntimePlatform.WebGLPlayer
            ? manager.GetComponent<SimpleWebTransport>()
            : manager.GetComponent<KcpTransport>();
    }

    #endregion

    #region Editor

#if UNITY_EDITOR
    private void SetupEditorMode(IContainerBuilder builder)
    {
        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Contains("Server"))
        {
            LoggerUtility.Info("<color=yellow>[Network] Using <color=white>editor-specific</color> configuration</color>");
            builder.RegisterEntryPoint<EntryPointServer>()
            .As<EntryPointServer>();
        }
        else if (tags.Contains("Client"))
        {
            LoggerUtility.Info("<color=yellow>[Network] Using <color=white>editor-specific</color> configuration</color>");
            builder.RegisterEntryPoint<EntryPointClient>()
            .As<EntryPointClient>();
        }
        else
        {
            SetupBuildMode(builder);
        }
    }
#endif

    #endregion

    #region Build

    private void SetupBuildMode(IContainerBuilder builder)
    {
        LoggerUtility.Info("<color=yellow>[Network] Using <color=white>build-specific</color> configuration</color>");
        switch (_networkConfig.networkType)
        {
            case NetworkType.Server:
                builder.RegisterEntryPoint<EntryPointServer>()
                .As<EntryPointServer>();
                break;

            case NetworkType.Client:
                builder.RegisterEntryPoint<EntryPointClient>()
                .As<EntryPointClient>();
                break;

            default:
                throw new ArgumentException($"Invalid network type: {_networkConfig.networkType}");
        }
    }

    #endregion
}
