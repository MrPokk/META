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
        ValidateField();
        RegisterAllConfigs(builder);
        RegisterLogger(builder);
        RegisterSceneManagement(builder);
        RegisterNetworkInfrastructure(builder);
        RegisterEcsSystem(builder);
        RegisterServiceInject(builder);
    }

    private void ValidateField()
    {
        try
        {
            if (_loggerConfig == null)
            {
                throw new Exception($"{nameof(_loggerConfig)} is not assigned in the EntryPointProject");
            }

            if (_networkConfig == null)
            {
                throw new Exception($"{nameof(_networkConfig)} is not assigned in the EntryPointProject");
            }

            if (_sceneConfig == null)
            {
                throw new Exception($"{nameof(_sceneConfig)} is not assigned in the EntryPointProject");
            }

            if (_inputSystemUIInputModule == null)
            {
                throw new Exception($"{nameof(_inputSystemUIInputModule)} is not assigned in the EntryPointProject");
            }

            if (_visualEffectService == null)
            {
                throw new Exception($"{nameof(_visualEffectService)} is not assigned in the EntryPointProject");
            }
        }
        catch (Exception)
        {
            throw new Exception("Configurations are not assigned in the EntryPointProject");
        }
    }

    #region Dependency Registration

    private void RegisterAllConfigs(IContainerBuilder builder)
    {
        builder.RegisterInstance(_loggerConfig).AsSelf();
        builder.RegisterInstance(_networkConfig).AsSelf();
        builder.RegisterInstance(_sceneConfig).AsSelf();
    }

    private void RegisterLogger(IContainerBuilder builder)
    {
        builder.RegisterInstance<LoggerUtility>(new(_loggerConfig, _networkConfig)).AsSelf();
    }

    private void RegisterSceneManagement(IContainerBuilder builder)
    {
        builder.RegisterInstance<SceneLoader>(new(_sceneConfig)).AsSelf();
        SceneLoader.LoadScene(SceneTypes.EntryPoint);
    }

    private void RegisterEcsSystem(IContainerBuilder builder)
    {
        var ecsManager = CreateEcsManager();
        builder.RegisterComponent(ecsManager)
               .As<EcsNetworkUnity>()
               .AsImplementedInterfaces();
    }

    private void RegisterServiceInject(IContainerBuilder builder)
    {
        builder.Register<TeleportService>(Lifetime.Singleton);
        builder.Register<QuestionService>(Lifetime.Singleton);
    }

    private void RegisterUIEntryPoint(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<UIEntryPoint>();
        DontDestroyOnLoad(Instantiate(_inputSystemUIInputModule).gameObject);
    }

    #endregion

    #region Component Creation

    private VFXService CreateVFXService(VFXService prefabVfx)
    {
        var vfxService = Instantiate(prefabVfx);
        DontDestroyOnLoad(vfxService.gameObject);
        return vfxService;
    }

    private EcsNetworkUnity CreateEcsManager()
    {
        var ecsManager = new GameObject("[EcsManager]", typeof(EcsNetworkUnity))
            .GetComponent<EcsNetworkUnity>();
        DontDestroyOnLoad(ecsManager.gameObject);
        return ecsManager;
    }

    #endregion

    #region Configuration Network

    private void RegisterNetworkInfrastructure(IContainerBuilder builder)
    {
        var networkManager = CreateNetworkManager();
        builder.RegisterComponent(networkManager)
               .As<NetworkManager>()
               .AsImplementedInterfaces();

        RegisterNetworkProviders(builder);
        RegisterPlatformSpecificEntryPoints(builder);
    }

    private NetworkManager CreateNetworkManager()
    {
        var manager = new GameObject("[NetworkManager]",
                typeof(KcpTransport),
                typeof(SimpleWebTransport),
                typeof(SceneInterestManagement),
                typeof(NetworkManager))
            .GetComponent<NetworkManager>();

        SetupSpawnPrefabs(manager);
        SetupPlatformSpecificTransport(manager);
        DontDestroyOnLoad(manager.gameObject);
        return manager;
    }

    private void SetupSpawnPrefabs(NetworkManager networkManager)
    {
        var entityPrefabs = Resources.LoadAll<GameObject>(PathProject.ENTITIES);
        foreach (var prefab in entityPrefabs)
        {
            var hasNetworkIdentity = prefab.TryGetComponent<NetworkIdentity>(out var _);
            var hasMonoProvider = prefab.TryGetComponent<MonoProvider>(out var _);

            if (hasNetworkIdentity && hasMonoProvider)
                networkManager.spawnPrefabs.Add(prefab);
        }
    }

    private void SetupPlatformSpecificTransport(NetworkManager manager)
    {
        manager.transport = Application.platform == RuntimePlatform.WebGLPlayer
            ? manager.GetComponent<SimpleWebTransport>()
            : manager.GetComponent<KcpTransport>();
    }

    private void RegisterNetworkProviders(IContainerBuilder builder)
    {
        var providerTypes = ReflectionUtility.FindAllAssignments<IProviderHandler>();
        foreach (var type in providerTypes)
        {
            builder.Register(type, Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }

        builder.Register<ConnectionInfo>(Lifetime.Singleton);
    }

    private void RegisterPlatformSpecificEntryPoints(IContainerBuilder builder)
    {
        builder.RegisterInstance<NetworkUtility>(new(_networkConfig)).AsSelf();

#if UNITY_EDITOR
        ConfigureEditorMode(builder);
#else
        ConfigureBuildMode(builder);
#endif
    }

    private void RegisterAppropriateEntryPoint(IContainerBuilder builder, NetworkType networkType)
    {
        if (networkType == NetworkType.Client)
        {
            builder.RegisterEntryPoint<EntryPointClient>().AsSelf();
            builder.RegisterInstance<SaveService>(new());
            builder.RegisterInstance(CreateVFXService(_visualEffectService)).AsSelf();
            RegisterUIEntryPoint(builder);
        }
        else if (networkType == NetworkType.Server)
        {
            builder.RegisterEntryPoint<EntryPointServer>().AsSelf();
        }
    }

#if UNITY_EDITOR
    private void ConfigureEditorMode(IContainerBuilder builder)
    {
        var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;

        if (!tags.Contains("Server") && !tags.Contains("Client"))
        {
            ConfigureBuildMode(builder);
            return;
        }

        var networkType = tags.Contains("Client") ? NetworkType.Client : NetworkType.Server;
        LoggerUtility.Info($"<color=yellow>[Network] Editor mode: <color=white>{networkType}</color></color>");

        RegisterAppropriateEntryPoint(builder, networkType);
    }
#endif

    private void ConfigureBuildMode(IContainerBuilder builder)
    {
        LoggerUtility.Info($"<color=yellow>[Network] Build mode: <color=white>{NetworkUtility.Type}</color></color>");

        RegisterAppropriateEntryPoint(builder, NetworkUtility.Type);
    }

    #endregion
}
