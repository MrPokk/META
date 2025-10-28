using UnityEngine;
using VContainer.Unity;

public class StartRoomEntryPoint : LifetimeScope
{
    void Start()
    {
        ObjectNetworkProvider.Spawn<PlayerProvider>(Vector3.zero, Quaternion.identity);
    }
}
