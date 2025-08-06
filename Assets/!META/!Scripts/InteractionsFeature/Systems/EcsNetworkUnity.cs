using BitterECS.Core;
using BitterECS.Core.Integration;
using Mirror;

public class EcsNetworkUnity : EcsUnityRoot
{
    public override void Run()
    {
        base.Run();

        if (NetworkServer.active)
            EcsSystems.Run<IServerConnectedRun>(system => system.Run());
        else if (NetworkClient.active)
            EcsSystems.Run<IClientConnectedRun>(system => system.Run());
    }

    public override void FixedRun()
    {
        base.FixedRun();

        if (NetworkServer.active)
            EcsSystems.Run<IServerConnectedFixedRun>(system => system.FixedRun());
        else if (NetworkClient.active)
            EcsSystems.Run<IClientConnectedFixedRun>(system => system.FixedRun());
        
    }
}
