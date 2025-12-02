using UnityEngine;

public class StartRoomEntryPoint : EntryPointFloors
{
    private void Start()
    {
        ObjectNetworkProvider.Spawn<PlayerProvider>(_playerSpawnPoint.Position, Quaternion.identity);
    }
}
