using System;
using System.Linq;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using BitterECS.Utility;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using BitterECS.Extra;

#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EntryPointProject : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private LoggerConfig _loggerConfig;
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        InitializeLogger();
        RegisterCoreDependencies(builder);
        RegisterPlatformSpecificEntryPoints(builder);
    }

    #region Core Initialization

    private void InitializeLogger()
    {
        LoggerUtility.Initialize(_loggerConfig);
    }

    private void RegisterCoreDependencies(IContainerBuilder builder)
    {
        RegisterConfigurations(builder);
        RegisterSceneManagement(builder);
        RegisterNetworkInfrastructure(builder);
        RegisterEcsSystem(builder);
        RegisterProviders(builder);
    }

    #endregion

    #region Dependency Registration

    private void RegisterConfigurations(IContainerBuilder builder)
    {
        builder.RegisterInstance(_networkConfig);
        builder.RegisterInstance(_sceneConfig);
    }

    private void RegisterSceneManagement(IContainerBuilder builder)
    {
        var sceneLoader = CreateSceneLoader();
        builder.RegisterInstance(sceneLoader);
    }

    private void RegisterNetworkInfrastructure(IContainerBuilder builder)
    {
        var networkManager = CreateNetworkManager();
        builder.RegisterComponent(networkManager)
               .As<NetworkManager>()
               .AsImplementedInterfaces();
    }

    private void RegisterEcsSystem(IContainerBuilder builder)
    {
        var ecsManager = CreateEcsManager();
        builder.RegisterComponent(ecsManager)
               .As<EcsNetworkUnity>()
               .AsImplementedInterfaces();
    }

    private void RegisterProviders(IContainerBuilder builder)
    {
        var providerTypes = ReflectionUtility.FindAllAssignments<IProviderHandler>();
        foreach (var type in providerTypes)
        {
            builder.Register(type, Lifetime.Singleton).As(type).AsImplementedInterfaces();
        }

        builder.Register<ConnectionInfo>(Lifetime.Singleton);
    }

    private void RegisterUIEntryPoint(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<UIEntryPoint>();
    }

    #endregion

    #region Component Creation

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

        ConfigureNetworkManager(manager);
        DontDestroyOnLoad(manager.gameObject);
        return manager;
    }

    private void ConfigureNetworkManager(NetworkManager manager)
    {
        RegisterSpawnPrefabs(manager);
        SetupPlatformSpecificTransport(manager);
        LoadServerScenes();
    }

    private void RegisterSpawnPrefabs(NetworkManager manager)
    {
        var entityPrefabs = Resources.LoadAll<GameObject>(PathProject.ENTITIES);
        foreach (var prefab in entityPrefabs)
        {
            var hasNetworkIdentity = prefab.TryGetComponent<NetworkIdentity>(out var _);
            var hasMonoProvider = prefab.TryGetComponent<MonoProvider>(out var _);

            if (hasNetworkIdentity && hasMonoProvider)
                manager.spawnPrefabs.Add(prefab.gameObject);
        }
    }

    private void SetupPlatformSpecificTransport(NetworkManager manager)
    {
        manager.transport = Application.platform == RuntimePlatform.WebGLPlayer
            ? manager.GetComponent<SimpleWebTransport>()
            : manager.GetComponent<KcpTransport>();
    }

    private void LoadServerScenes()
    {
        var serverScenes = _sceneConfig.GetServerLoadScenes();
        foreach (var scene in serverScenes)
        {
            SceneLoader.LoadScene(scene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
        }
    }

    #endregion

    #region Platform-Specific Configuration

    private void RegisterPlatformSpecificEntryPoints(IContainerBuilder builder)
    {
#if UNITY_EDITOR
        ConfigureEditorMode(builder);
#else
        ConfigureBuildMode(builder);
#endif
    }

    private void RegisterAppropriateEntryPoint(IContainerBuilder builder, bool isClient)
    {
        if (isClient)
        {
            builder.RegisterEntryPoint<EntryPointClient>().As<EntryPointClient>(); ;
            RegisterUIEntryPoint(builder);
            RegisterClientGameplayLogic(builder);
        }
        else
        {
            builder.RegisterEntryPoint<EntryPointServer>().As<EntryPointServer>();
        }
    }

    private void RegisterClientGameplayLogic(IContainerBuilder builder)
    {
        builder.Register<TeleportService>(Lifetime.Singleton);
    }


#if UNITY_EDITOR
    private void ConfigureEditorMode(IContainerBuilder builder)
    {
        var tags = CurrentPlayer.ReadOnlyTags();

        if (tags.Contains("Server") || tags.Contains("Client"))
        {
            var isClient = tags.Contains("Client");
            var mode = isClient ? "Client" : "Server";
            LoggerUtility.Info($"<color=yellow>[Network] Editor mode: <color=white>{mode}</color></color>");

            RegisterAppropriateEntryPoint(builder, isClient);
        }
        else
        {
            ConfigureBuildMode(builder);
        }
    }
#endif

    private void ConfigureBuildMode(IContainerBuilder builder)
    {
        var isClient = _networkConfig.networkType == NetworkType.Client;
        var mode = isClient ? "Client" : "Server";
        LoggerUtility.Info($"<color=yellow>[Network] Build mode: <color=white>{mode}</color></color>");

        RegisterAppropriateEntryPoint(builder, isClient);
    }

    #endregion
}
