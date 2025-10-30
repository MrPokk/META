using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using System;
using Object = UnityEngine.Object;

public class ObjectNetworkProvider : IProviderHandler
{
    public static void Spawn<T>(Vector3 position, Quaternion rotation) where T : MonoProvider
    {
        NetworkUtility.SendMessage(new SyncObjectSpawn(typeof(T), position, rotation));
    }

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SyncObjectSpawn>(OnClientSync);
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncObjectSpawn>(OnServerSync);
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        if (NetworkClient.spawned.TryGetValue(spawn.assetId, out var clientGameObject))
        {
            if (clientGameObject.TryGetComponent<MonoProvider>(out var provider))
            {
                provider.Entity.Add<ControllableComponent>(new());
            }
        }
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        var entityPrefab = FindEntityPrefab(spawn.entity.Type);
        if (entityPrefab == null) return;

        var goInstance = CreateEntityInstance(entityPrefab, conn);
        MoveEntityToClientScene(goInstance, conn);

        var identity = goInstance.GetComponent<NetworkIdentity>();

        if (identity.TryGetComponent<PlayerProvider>(out var _))
        {
            RegisterPlayerForConnection(conn, goInstance);
        }
        else
        {
            RegisterObjectForConnection(conn, goInstance);
        }

        SendSpawnConfirmation(conn, spawn, identity);
        TrackClientEntity(conn, identity.netId);
    }

    private GameObject FindEntityPrefab(Type entityType)
    {
        var spawnToPrefab = NetworkManager.singleton.spawnPrefabs;
        var entityPrefab = spawnToPrefab.Find(e => e.gameObject.TryGetComponent(entityType, out var entity));

        if (entityPrefab == null)
        {
            LoggerUtility.Error($"Prefab with component {entityType} not found in spawnPrefabs");
            return null;
        }

        return entityPrefab.gameObject;
    }

    private GameObject CreateEntityInstance(GameObject prefab, NetworkConnectionToClient conn)
    {
        var instance = Object.Instantiate(prefab);
        instance.name = $"{prefab.name} [{conn.connectionId}]";
        return instance;
    }

    private void MoveEntityToClientScene(GameObject entity, NetworkConnectionToClient conn)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(conn, out var sceneType))
        {
            SceneManager.MoveGameObjectToScene(entity, SceneConfig.GetSceneToType(sceneType));
        }
    }

    private void RegisterObjectForConnection(NetworkConnectionToClient conn, GameObject networkObject) => NetworkServer.Spawn(networkObject, conn);
    private void RegisterPlayerForConnection(NetworkConnectionToClient conn, GameObject playerObject) => NetworkServer.AddPlayerForConnection(conn, playerObject);

    private void SendSpawnConfirmation(NetworkConnectionToClient conn, SyncObjectSpawn originalSpawn, NetworkIdentity identity) =>
    conn.Send(new SyncObjectSpawn(originalSpawn, identity.netId));

    private void TrackClientEntity(NetworkConnectionToClient conn, uint netId) => ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new() { netId }).Add(netId);
}
