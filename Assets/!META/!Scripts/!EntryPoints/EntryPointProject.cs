using System;
using System.Linq;
using kcp2k;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;
using VContainer.Unity;

#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EntryPointProject : LifetimeScope
{
    [Header("<size=16>Configs</size>")]
    [SerializeField] private NetworkConfig _networkConfig;
    [SerializeField] private SceneConfig _sceneConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        RegisterUtility(builder);
        RegisterCoreComponents(builder);

#if UNITY_EDITOR
        if (TryRegisterEditorNetworkDependencies(builder))
        {
            Debug.Log("<color=yellow>[Network] Using editor-specific configuration</color>");
            return;
        }
#endif
        Debug.Log("<color=yellow>[Network] Using build-specific configuration</color>");
        RegisterBuildNetworkDependencies(builder);
    }

    private void RegisterUtility(IContainerBuilder builder)
    {
        builder.RegisterInstance(SetupSceneLoader());
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

#if UNITY_EDITOR
    private bool TryRegisterEditorNetworkDependencies(IContainerBuilder builder)
    {
        var tags = CurrentPlayer.ReadOnlyTags();
        if (tags.Contains("Server"))
        {
            builder.RegisterEntryPoint<EntryPointServer>().AsSelf();
            return true;
        }
        else if (tags.Contains("Client"))
        {
            builder.RegisterEntryPoint<EntryPointClient>().AsSelf();
            return true;
        }

        return false;
    }
#endif

    private void RegisterBuildNetworkDependencies(IContainerBuilder builder)
    {
        switch (_networkConfig.networkType)
        {
            case NetworkType.Server:
                builder.RegisterEntryPoint<EntryPointServer>().AsSelf();
                break;
            case NetworkType.Client:
                builder.RegisterEntryPoint<EntryPointClient>().AsSelf();
                break;
            default:
                throw new Exception($"Invalid network type: {_networkConfig.networkType}");
        }
    }

    private SceneLoader SetupSceneLoader()
    {
        var sceneLoader = SceneLoader.Initialize(_sceneConfig);
        SceneLoader.LoadScene(SceneTypes.EntryPoint);
        return sceneLoader;
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
