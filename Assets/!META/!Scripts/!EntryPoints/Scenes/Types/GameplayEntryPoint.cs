using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        SyncSceneNetworkProvider.SendRequest();
        ObjectNetworkProvider.SendRequest<PlayerEntity, PlayerView>(new(Vector3.zero, Quaternion.identity));
    }
}
