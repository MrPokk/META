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


#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EntryPointProject : LifetimeScope
{
    [Header("Configurations")]
    [SerializeField] private LoggerConfig _loggerConfig;
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;
    [SerializeField] private InputSystemUIInputModule _inputSystemUIInputModule;

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
            builder.Register(type, Lifetime.Singleton).As(type).AsImplementedInterfaces();
        }

        builder.Register<ConnectionInfo>(Lifetime.Singleton);
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

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок инициализации логгера
    // Критично, так как без логгера мы не сможем отслеживать другие ошибки
    private void InitializeLogger()
    {
        try
        {
            LoggerUtility.Initialize(_loggerConfig);
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку инициализации логгера
            LoggerUtility.Critical($"Failed to initialize logger: {ex.Message}\n{ex.StackTrace}");
            Debug.LogError($"Failed to initialize logger: {ex.Message}");
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок создания загрузчика сцен
    // Может возникнуть при проблемах с конфигурацией сцен или загрузкой
    private SceneLoader CreateSceneLoader()
    {
        try
        {
            var loader = new SceneLoader();
            loader.Initialize(_sceneConfig);
            SceneLoader.LoadScene(SceneTypes.EntryPoint);
            return loader;
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку создания загрузчика сцен
            LoggerUtility.Critical($"Failed to create scene loader: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок создания ECS менеджера
    // Включает проверку на null после создания компонента
    private EcsNetworkUnity CreateEcsManager()
    {
        try
        {
            var ecsManager = new GameObject("[EcsManager]", typeof(EcsNetworkUnity))
                .GetComponent<EcsNetworkUnity>();
            
            // Проверка на null - компонент может не создаться
            if (ecsManager == null)
            {
                LoggerUtility.Error("Failed to create EcsNetworkUnity component");
                throw new NullReferenceException("EcsNetworkUnity component is null");
            }
            
            DontDestroyOnLoad(ecsManager.gameObject);
            return ecsManager;
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку создания ECS менеджера
            LoggerUtility.Critical($"Failed to create ECS manager: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок создания сетевого менеджера
    // Включает проверку на null после создания компонента
    private NetworkManager CreateNetworkManager()
    {
        try
        {
            var manager = new GameObject("[NetworkManager]",
                    typeof(KcpTransport),
                    typeof(SimpleWebTransport),
                    typeof(SceneInterestManagement),
                    typeof(NetworkManager))
                .GetComponent<NetworkManager>();

            // Проверка на null - компонент может не создаться
            if (manager == null)
            {
                LoggerUtility.Error("Failed to create NetworkManager component");
                throw new NullReferenceException("NetworkManager component is null");
            }

            ConfigureNetworkManager(manager);
            DontDestroyOnLoad(manager.gameObject);
            return manager;
        }
        catch (Exception ex)
        {
            // Логируем критическую ошибку создания сетевого менеджера
            LoggerUtility.Critical($"Failed to create network manager: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private void ConfigureNetworkManager(NetworkManager manager)
    {
        RegisterSpawnPrefabs(manager);
        SetupPlatformSpecificTransport(manager);
        LoadServerScenes();
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок регистрации префабов для спавна
    // Включает проверки на пустые коллекции и null префабы
    private void RegisterSpawnPrefabs(NetworkManager manager)
    {
        try
        {
            var entityPrefabs = Resources.LoadAll<GameObject>(PathProject.ENTITIES);
            
            // Проверка на пустую коллекцию префабов
            if (entityPrefabs == null || entityPrefabs.Length == 0)
            {
                LoggerUtility.Warning($"No entity prefabs found at path: {PathProject.ENTITIES}");
                return;
            }

            foreach (var prefab in entityPrefabs)
            {
                // Проверка на null префаб в коллекции
                if (prefab == null)
                {
                    LoggerUtility.Warning("Null prefab found in Resources.LoadAll result");
                    continue;
                }

                var hasNetworkIdentity = prefab.TryGetComponent<NetworkIdentity>(out var _);
                var hasMonoProvider = prefab.TryGetComponent<MonoProvider>(out var _);

                if (hasNetworkIdentity && hasMonoProvider)
                    manager.spawnPrefabs.Add(prefab.gameObject);
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку регистрации префабов
            LoggerUtility.Error($"Failed to register spawn prefabs: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void SetupPlatformSpecificTransport(NetworkManager manager)
    {
        manager.transport = Application.platform == RuntimePlatform.WebGLPlayer
            ? manager.GetComponent<SimpleWebTransport>()
            : manager.GetComponent<KcpTransport>();
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок загрузки серверных сцен
    // Включает проверку на пустую коллекцию и обработку ошибок для каждой сцены отдельно
    private void LoadServerScenes()
    {
        try
        {
            var serverScenes = _sceneConfig.GetServerLoadScenes();
            
            // Проверка на пустую коллекцию сцен
            if (serverScenes == null || serverScenes.Count == 0)
            {
                LoggerUtility.Warning("No server scenes configured");
                return;
            }

            // Обрабатываем каждую сцену отдельно, чтобы одна ошибка не блокировала остальные
            foreach (var scene in serverScenes)
            {
                try
                {
                    var sceneToServer = SceneLoader.LoadScene(scene, new LoadSceneParameters
                    {
                        loadSceneMode = LoadSceneMode.Additive
                    });
                    SceneLoader.AddServerScene(scene, sceneToServer);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку загрузки конкретной сцены, но продолжаем загрузку остальных
                    LoggerUtility.Error($"Failed to load server scene {scene}: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
        catch (Exception ex)
        {
            // Логируем общую ошибку загрузки серверных сцен
            LoggerUtility.Error($"Failed to load server scenes: {ex.Message}\n{ex.StackTrace}");
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
        var isClient = _networkConfig.NetworkType == NetworkType.Client;
        var mode = isClient ? "Client" : "Server";
        LoggerUtility.Info($"<color=yellow>[Network] Build mode: <color=white>{mode}</color></color>");

        RegisterAppropriateEntryPoint(builder, isClient);
    }

    #endregion
}
