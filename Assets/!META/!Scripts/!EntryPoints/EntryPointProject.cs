using System;
using System.Linq;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using BitterECS.Core;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using BitterECS.Extra;
using UnityEngine.InputSystem.UI;

public class EntryPointProject : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private LoggerConfig _loggerConfig;
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;

    [Header("UI")]
    [SerializeField] private InputSystemUIInputModule _inputSystemUIInputModule;

    [Header("Services")]
    [SerializeField] private VFXService _visualEffectService;

    protected override void Configure(IContainerBuilder builder)
    {
        InitializeLogger();
        RegisterCoreDependencies(builder);
        RegisterPlatformSpecificEntryPoints(builder);
    }

    #region Core Initialization

    private void RegisterCoreDependencies(IContainerBuilder builder)
    {
        RegisterConfigurations(builder);
        RegisterSceneManagement(builder);
        RegisterSettings(builder);
        RegisterNetworkInfrastructure(builder);
        RegisterEcsSystem(builder);
        RegisterProviders(builder);
        RegisterServiceInject(builder);
    }

    #endregion

    #region Dependency Registration

    private void RegisterConfigurations(IContainerBuilder builder)
    {
        builder.RegisterInstance(_networkConfig);
        builder.RegisterInstance(_sceneConfig);
    }

    private void RegisterSettings(IContainerBuilder builder)
    {
        builder.Register<SettingGlobal>(Lifetime.Singleton);
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
            builder.Register(type, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }

        builder.Register<ConnectionInfo>(Lifetime.Singleton);
    }

    private void RegisterServiceInject(IContainerBuilder builder)
    {
        builder.Register<TeleportService>(Lifetime.Singleton);
        builder.Register<QuestionService>(Lifetime.Singleton);
        builder.RegisterInstance(CreateVFXService(_visualEffectService)).AsSelf();
    }

    private void RegisterUIEntryPoint(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<UIEntryPoint>();
        DontDestroyOnLoad(Instantiate(_inputSystemUIInputModule).gameObject);
    }

    #endregion

    #region Component Creation

    private void InitializeLogger()
    {
        LoggerUtility.Initialize(_loggerConfig);
    }

    private VFXService CreateVFXService(VFXService prefabVfx)
    {
        var vfxService = Instantiate(prefabVfx);
        DontDestroyOnLoad(vfxService.gameObject);
        return vfxService;
    }

    private SceneLoader CreateSceneLoader()
    {
        var loader = new SceneLoader();
        loader.Initialize(_sceneConfig);
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
            var sceneToServer = SceneLoader.LoadScene(scene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive
            });
            SceneLoader.AddServerScene(scene, sceneToServer);
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
        }
        else
        {
            builder.RegisterEntryPoint<EntryPointServer>().As<EntryPointServer>();
        }
    }

#if UNITY_EDITOR
    private void ConfigureEditorMode(IContainerBuilder builder)
    {
        var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;

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
        var isClient = _networkConfig.NetworkType == NetworkType.Client;
        var mode = isClient ? "Client" : "Server";
        LoggerUtility.Info($"<color=yellow>[Network] Build mode: <color=white>{mode}</color></color>");

        RegisterAppropriateEntryPoint(builder, isClient);
    }

    #endregion
}
