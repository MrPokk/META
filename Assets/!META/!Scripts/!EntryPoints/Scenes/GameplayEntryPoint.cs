using Mirror;
using UnityEngine;
using VContainer.Unity;

public class GameplayEntryPoint : LifetimeScope
{
    [SerializeField] private NetworkBehaviour playerPrefab;

    private void Start()
    {
        var networkManager = NetworkManager.singleton;
        networkManager.playerPrefab = playerPrefab.gameObject;
        
        NetworkClient.Send(new SpawnRequestMessage());
    }
}
