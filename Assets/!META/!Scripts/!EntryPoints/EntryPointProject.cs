using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointProject : LifetimeScope
{
    [SerializeField] private NetworkServerConfig _networkServerConfig;
    [SerializeField] private NetworkClientConfig _networkClientConfig;
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
               .As<NetworkManager>()
               .AsImplementedInterfaces();

        var ecsManager = SetupEcsManager();
        builder.RegisterComponent(ecsManager)
               .As<EcsNetworkUnity>()
               .AsImplementedInterfaces();

        builder.RegisterInstance(_sceneConfig);
    }

    private void RegisterNetworkDependencies(IContainerBuilder builder)
    {
#if DEDICATED_SERVER || UNITY_EDITOR
        builder.RegisterInstance(_networkServerConfig);
        builder.RegisterEntryPoint<EntryPointServer>()
               .AsSelf();
#if !DEDICATED_SERVER || UNITY_EDITOR
        builder.RegisterInstance(_networkClientConfig);
        builder.RegisterEntryPoint<EntryPointClient>()
               .AsSelf();
#endif 
#endif
    }

    private EcsNetworkUnity SetupEcsManager()
    {
        var ecsManager = new GameObject("[EcsEntryPoint]", typeof(EcsNetworkUnity))
            .GetComponent<EcsNetworkUnity>();
        DontDestroyOnLoad(ecsManager.gameObject);
        return ecsManager;
    }

    private NetworkManager SetupNetworkManager()
    {
        var networkManager = new GameObject("[NetworkManager]",
                typeof(KcpTransport),
                typeof(SimpleWebTransport),
                typeof(NetworkManager))
            .GetComponent<NetworkManager>();
        DontDestroyOnLoad(networkManager.gameObject);
        return networkManager;
    }
}
