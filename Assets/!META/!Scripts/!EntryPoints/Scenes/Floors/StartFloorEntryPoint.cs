using System.Threading.Tasks;
using UnityEngine;

public class StartRoomEntryPoint : EntryPointFloors
{
    private async void Start()
    {
        await ObjectNetworkProvider.Spawn<PlayerProvider>(_playerSpawnPoint.Position, Quaternion.identity);
    }
}
