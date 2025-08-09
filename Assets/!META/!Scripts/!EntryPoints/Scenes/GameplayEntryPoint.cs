using UnityEngine;

public class GameplayEntryPoint : EntryPointScene
{
    private void Start()
    {
        ObjectNetworkProvide.ClientRequestSpawnObject(new(IDConstPrefabs.PLAYER_TEST, Vector3.zero, Quaternion.identity));
    }
}
