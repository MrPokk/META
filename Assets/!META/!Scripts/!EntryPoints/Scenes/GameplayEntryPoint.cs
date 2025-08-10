using BitterECS.Core;
using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    protected override void Bootstrap()
    {
        ObjectNetworkProvide.ClientRequestSpawnObject<PlayerEntity, PlayerView>(new(Vector3.zero, Quaternion.identity));
    }
}
