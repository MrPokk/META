using UnityEngine;
using VContainer.Unity;

public class StartRoomEntryPoint : LifetimeScope
{
   [SerializeField] private Transform _playerSpawnPoint;

    void Start()
    {
        ObjectNetworkProvider.Spawn<PlayerProvider>(_playerSpawnPoint.localPosition, Quaternion.identity);   
    }
}
