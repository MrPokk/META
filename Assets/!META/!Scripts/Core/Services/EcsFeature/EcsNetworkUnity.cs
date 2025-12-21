using System;
using BitterECS.Core;
using VContainer;
using System.Linq;
using BitterECS.Integration;


#if UNITY_EDITOR

#endif

public class EcsNetworkUnity : EcsUnityRoot
{
    [Inject]
    private NetworkConfig _networkConfig;

    protected override void Update()
    {
        base.Update();

#if UNITY_EDITOR
        RunHandlingInEditor();
#else
        RunHandlingInBuild();
#endif
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

#if UNITY_EDITOR
        FixedRunHandlingInEditor();
#else
        FixedRunHandlingInBuild();
#endif
    }

#if UNITY_EDITOR
    private void RunHandlingInEditor()
    {
        var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;
        if (tags.Contains("Server"))
        {
            EcsSystems.Run<IServerConnectedRun>(system => system.Run());
        }
        else if (tags.Contains("Client"))
        {
            EcsSystems.Run<IClientConnectedRun>(system => system.Run());
        }
        else
        {
            RunHandlingInBuild();
        }
    }

    private void FixedRunHandlingInEditor()
    {
        var tags = Unity.Multiplayer.PlayMode.CurrentPlayer.Tags;
        if (tags.Contains("Server"))
        {
            EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
        }
        else if (tags.Contains("Client"))
        {
            EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
        }
        else
        {
            FixedRunHandlingInBuild();
        }
    }
#endif

    private void RunHandlingInBuild()
    {
        switch (_networkConfig.NetworkType)
        {
            case NetworkType.Server:
                EcsSystems.Run<IServerConnectedRun>(system => system.Run());
                break;
            case NetworkType.Client:
                EcsSystems.Run<IClientConnectedRun>(system => system.Run());
                break;
            default:
                throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
        }
    }

    private void FixedRunHandlingInBuild()
    {
        switch (_networkConfig.NetworkType)
        {
            case NetworkType.Server:
                EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
                break;
            case NetworkType.Client:
                EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
                break;
            default:
                throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
        }
    }
}
