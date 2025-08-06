using BitterECS.Core;
using BitterECS.Core.Integration;

public class EcsNetworkUnity : EcsUnityRoot
{
    public override void Run()
    {
        base.Run();
#if DEDICATED_SERVER || UNITY_EDITOR
        EcsSystems.Run<IServerConnectedRun>(system => system.Run());
#endif
#if  !DEDICATED_SERVER || UNITY_EDITOR
        EcsSystems.Run<IClientConnectedRun>(system => system.Run());
#endif
    }

    public override void FixedRun()
    {
        base.FixedRun();

#if DEDICATED_SERVER || UNITY_EDITOR
        EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
#endif
#if  !DEDICATED_SERVER || UNITY_EDITOR
        EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
#endif
    }
}
