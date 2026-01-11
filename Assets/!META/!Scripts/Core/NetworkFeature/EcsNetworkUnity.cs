using System;
using BitterECS.Core;
using VContainer;
using System.Linq;
using BitterECS.Integration;

public class EcsNetworkUnity : EcsUnityRoot
{
    private NetworkConfig _networkConfig;

    [Inject]
    public void Configure(NetworkConfig networkConfig)
    {
        _networkConfig = networkConfig;
    }

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
        try
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
        catch (Exception)
        {
            throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
        }
    }

    private void FixedRunHandlingInBuild()
    {
        try
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
        catch (Exception)
        {
            throw new Exception($"Invalid network type: {_networkConfig.NetworkType}");
        }
    }
}
