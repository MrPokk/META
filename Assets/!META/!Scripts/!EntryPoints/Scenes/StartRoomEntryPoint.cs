using UnityEngine;
using VContainer;

public class StartRoomEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        ObjectNetworkProvider.Spawn<PlayerProvider>(Vector3.zero, Quaternion.identity);
    }
}
