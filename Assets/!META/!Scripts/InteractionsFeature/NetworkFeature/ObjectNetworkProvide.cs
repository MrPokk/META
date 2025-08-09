using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class ObjectNetworkProvide : IHandlerMessages
{
    private readonly NetworkObjectPrefabConfig _prefabConfig;
    private readonly SceneNetworkProvider _sceneNetworkProvider;

    [Inject]
    public ObjectNetworkProvide(NetworkObjectPrefabConfig prefabConfig, SceneNetworkProvider sceneNetworkProvider)
    {
        _prefabConfig = prefabConfig;
        _sceneNetworkProvider = sceneNetworkProvider;
    }

    #region Server

    [Server]
    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SpawnObjectRequestMessage>(OnServerSpawnObject);
        NetworkServer.RegisterHandler<DestroyObjectRequestMessage>(OnServerDestroyObject);
    }

    [Server]
    private void OnServerSpawnObject(NetworkConnectionToClient client, SpawnObjectRequestMessage message)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("NetworkServer is not active");
            return;
        }

        if (!_prefabConfig.ContainsPrefab(message.prefabId))
        {
            Debug.LogError($"Prefab with ID '{message.prefabId}' not registered");
            return;
        }

        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(client, () =>
        {
            SpawnObjectForClient(client, message);
        }));
    }


    [Server]
    private void SpawnObjectForClient(NetworkConnectionToClient client, SpawnObjectRequestMessage message)
    {
        if (!_prefabConfig.TryGetPrefabById(message.prefabId, out var prefab))
        {
            Debug.LogError($"Failed to spawn object: Prefab with ID '{message.prefabId}' not found");
            return;
        }

        var instance = Object.Instantiate(prefab, message.position, message.rotation);

        if (!_sceneNetworkProvider.TryGetCurrentSceneToClient(client, out var valueScene))
        {
            Debug.LogError("Failed to get first scene to load");
            return;
        }

        SceneManager.MoveGameObjectToScene(instance,
        SceneManager.GetSceneByName(valueScene.sceneName));
        
        NetworkServer.Spawn(instance, client);
    }

    [Server]
    private void SpawnObjectForAll(SpawnObjectRequestMessage message)
    {
        if (!_prefabConfig.TryGetPrefabById(message.prefabId, out var prefab))
        {
            Debug.LogError($"Failed to spawn object: Prefab with ID '{message.prefabId}' not found");
            return;
        }

        var instance = Object.Instantiate(prefab, message.position, message.rotation);
        NetworkServer.Spawn(instance);
    }

    [Server]
    private void OnServerDestroyObject(NetworkConnectionToClient client, DestroyObjectRequestMessage message)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("NetworkServer is not active");
            return;
        }

        if (NetworkServer.spawned.TryGetValue(message.netId, out var networkIdentity))
        {
            NetworkServer.Destroy(networkIdentity.gameObject);
        }
    }

    #endregion

    #region Client

    [Client]
    public static void ClientRequestSpawnObject(SpawnObjectRequestMessage message)
    {
        CoroutineUtility.Run(NetworkUtility.WaitingToConnect(NetworkClient.connection, () => OnClientSpanObject(message)));
    }

    [Client]
    public static void ClientRequestDestroyObject(uint netId)
    {
        NetworkClient.Send(new DestroyObjectRequestMessage { netId = netId });
    }

    [Client]
    public void HandlersClient()
    {
        _prefabConfig.RegisterAllPrefabs();
    }

    private static void OnClientSpanObject(SpawnObjectRequestMessage message)
    {
        NetworkClient.Send(message);
    }

    #endregion
}
