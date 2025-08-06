using BitterECS.Core.Integration;
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
        builder.RegisterInstance(SetupNetworkManager());
        builder.RegisterInstance(SetupEcs());

        SetupServer(builder);
        SetupClient(builder);
    }

    private EcsNetworkUnity SetupEcs()
    {
        var ecsManager = new GameObject("[EcsEntryPoint]",
            typeof(EcsNetworkUnity)).GetComponent<EcsNetworkUnity>();

        DontDestroyOnLoad(ecsManager.gameObject);
        return ecsManager;
    }

    private NetworkManager SetupNetworkManager()
    {
        var networkManager = new GameObject("[NetworkManager]",
            typeof(KcpTransport),
            typeof(SimpleWebTransport),
            typeof(NetworkManager),
            typeof(NetworkManagerHUD)).GetComponent<NetworkManager>();
        DontDestroyOnLoad(networkManager.gameObject);
        return networkManager;
    }

    private void SetupClient(IContainerBuilder builder)
    {
#if !DEDICATED_SERVER
        builder.RegisterInstance(_networkClientConfig);
        builder.RegisterInstance(_sceneConfig);
        builder.RegisterEntryPoint<EntryPointClient>().AsSelf();
#endif
    }

    private void SetupServer(IContainerBuilder builder)
    {
#if DEDICATED_SERVER || UNITY_EDITOR
        builder.RegisterInstance(_networkServerConfig);
        builder.RegisterEntryPoint<EntryPointServer>().AsSelf();
#endif
    }
}
