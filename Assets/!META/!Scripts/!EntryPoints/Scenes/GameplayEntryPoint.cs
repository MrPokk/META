using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        SyncSceneNetworkProvider.SyncStateScene();
        ObjectNetworkProvider.RequestSpawnObject<PlayerEntity, PlayerView>(new(Vector3.zero, Quaternion.identity));
    }
}
