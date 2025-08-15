using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        ObjectNetworkProvider.Spawn<PlayerEntity, PlayerView>(Vector3.zero, Quaternion.identity);
    }
}
