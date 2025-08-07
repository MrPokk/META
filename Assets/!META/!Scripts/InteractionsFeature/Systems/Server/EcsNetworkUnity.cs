using BitterECS.Core;
using BitterECS.Core.Integration;

public class EcsNetworkUnity : EcsUnityRoot
{
    public override void Run()
    {
        base.Run();
#if UNITY_EDITOR
        EcsSystems.Run<IServerConnectedRun>(system => system.Run());
        EcsSystems.Run<IClientConnectedRun>(system => system.Run());
#elif SERVER
        EcsSystems.Run<IServerConnectedRun>(system => system.Run());
#elif CLIENT
        EcsSystems.Run<IClientConnectedRun>(system => system.Run());
#endif
    }

    public override void FixedRun()
    {
        base.FixedRun();

#if UNITY_EDITOR
        EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
        EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
#elif SERVER
        EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
#elif CLIENT
        EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
#endif
    }
}
