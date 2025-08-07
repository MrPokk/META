using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointProject : LifetimeScope
{
    [Header("<size=16>Configs</size>")]
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;

    protected override async void Awake()
    {
        SceneLoader.Initialize(_sceneConfig);
        await SceneLoader.LoadSceneAsync(SceneTypes.EntryPoint);

        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        RegisterCoreComponents(builder);
        RegisterNetworkDependencies(builder);
    }

    private void RegisterCoreComponents(IContainerBuilder builder)
    {
        var networkManager = SetupNetworkManager();
        builder.RegisterComponent(networkManager)
               .As<OverrideNetworkManager>()
               .AsImplementedInterfaces();

        var ecsManager = SetupEcsManager();
        builder.RegisterComponent(ecsManager)
               .As<EcsNetworkUnity>()
               .AsImplementedInterfaces();

        var sceneManager = SetupSceneNetworkManager();
        builder.RegisterComponent(sceneManager)
               .As<SceneNetworkManager>()
               .AsImplementedInterfaces();

        builder.RegisterInstance(_networkConfig);
        builder.RegisterInstance(_sceneConfig);
    }

    private void RegisterNetworkDependencies(IContainerBuilder builder)
    {
#if UNITY_EDITOR
        builder.RegisterEntryPoint<EntryPointServer>()
               .AsSelf();

        builder.RegisterEntryPoint<EntryPointClient>()
               .AsSelf();
#elif SERVER
        builder.RegisterEntryPoint<EntryPointServer>()
               .AsSelf();
#elif CLIENT
        builder.RegisterEntryPoint<EntryPointClient>()
               .AsSelf();
#endif
    }


    private SceneNetworkManager SetupSceneNetworkManager()
    {
        var sceneManager = new GameObject("[SceneManager]", typeof(SceneNetworkManager))
            .GetComponent<SceneNetworkManager>();
        DontDestroyOnLoad(sceneManager.gameObject);
        return sceneManager;
    }

    private EcsNetworkUnity SetupEcsManager()
    {
        var ecsManager = new GameObject("[EcsEntryPoint]", typeof(EcsNetworkUnity))
            .GetComponent<EcsNetworkUnity>();
        DontDestroyOnLoad(ecsManager.gameObject);
        return ecsManager;
    }

    private OverrideNetworkManager SetupNetworkManager()
    {
        var networkManager = new GameObject("[NetworkManager]",
                typeof(KcpTransport),
                typeof(SimpleWebTransport),
                typeof(SceneInterestManagement),
                typeof(OverrideNetworkManager))
            .GetComponent<OverrideNetworkManager>();

        DontDestroyOnLoad(networkManager.gameObject);
        return networkManager;
    }
}
