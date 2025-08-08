using System;
using BitterECS.Core;
using BitterECS.Core.Integration;
using VContainer;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class EcsNetworkUnity : EcsUnityRoot
{
    [Inject]
    private NetworkConfig _networkConfig;

    public override void Run()
    {
        base.Run();

#if UNITY_EDITOR
        RunHandlingInEditor();
#else
        RunHandlingInBuild();
#endif
    }

    public override void FixedRun()
    {
        base.FixedRun();

#if UNITY_EDITOR
        FixedRunHandlingInEditor();
#else
        FixedRunHandlingInBuild();
#endif
    }

#if UNITY_EDITOR
    private void RunHandlingInEditor()
    {
        var tags = CurrentPlayer.ReadOnlyTags();

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
        var tags = CurrentPlayer.ReadOnlyTags();

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
        switch (_networkConfig.networkType)
        {
            case NetworkType.Server:
                EcsSystems.Run<IServerConnectedRun>(system => system.Run());
                break;
            case NetworkType.Client:
                EcsSystems.Run<IClientConnectedRun>(system => system.Run());
                break;
            default:
                throw new Exception($"Invalid network type: {_networkConfig.networkType}");
        }
    }

    private void FixedRunHandlingInBuild()
    {
        switch (_networkConfig.networkType)
        {
            case NetworkType.Server:
                EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
                break;
            case NetworkType.Client:
                EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
                break;
            default:
                throw new Exception($"Invalid network type: {_networkConfig.networkType}");
        }
    }
}
