using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        ObjectNetworkProvider.Spawn<PlayerProvider>(Vector3.zero, Quaternion.identity);
    }
}
