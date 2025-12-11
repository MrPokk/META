using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using System;
using Object = UnityEngine.Object;

public class ObjectNetworkProvider : IProviderHandler
{
    public static void Spawn<T>(Vector3 position, Quaternion rotation) where T : MonoProvider => NetworkUtility.SendMessage(new SyncObjectSpawn(typeof(T), position, rotation));

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
        if (NetworkClient.spawned.TryGetValue(spawn.netId, out var clientGameObject))
        {
            if (clientGameObject.TryGetComponent<MonoProvider>(out var provider))
            {
                provider.Entity.Add<ControllableComponent>(new());
            }
        }
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (IsHavePlayerIdentity(conn, spawn)) return;

        var entityPrefab = FindEntityPrefab(spawn.entity.Type);
        if (entityPrefab == null) return;

        var goInstance = CreateEntityInstance(spawn, entityPrefab, conn);
        MoveEntityToClientScene(goInstance, conn);

        var identity = goInstance.GetComponent<NetworkIdentity>();

        if (identity.TryGetComponent<PlayerProvider>(out var _))
        {
            LoggerUtility.Info($"Registering player for connection {conn}");
            RegisterPlayerForConnection(conn, goInstance);
        }
        else
        {
            LoggerUtility.Info("Registering object for connection");
            RegisterObjectForConnection(conn, goInstance);
        }

        SendSpawnConfirmation(conn, spawn, identity);
        TrackClientEntity(conn, identity);
    }

    private bool IsHavePlayerIdentity(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (!ConnectionInfo.PlayerEntityId.TryGetValue(conn, out var playerId))
        {
            return false;
        }

        RegisterPlayerForConnection(conn, playerId.gameObject);
        SendSpawnConfirmation(conn, spawn, playerId);
        TrackClientEntity(conn, playerId);
        return true;
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

    private GameObject CreateEntityInstance(in SyncObjectSpawn spawn, GameObject prefab, NetworkConnectionToClient conn)
    {
        var instance = Object.Instantiate(prefab, spawn.position, spawn.rotation);
        instance.name = $"{prefab.name} [{conn.connectionId}]";
        return instance;
    }

    private void MoveEntityToClientScene(GameObject entity, NetworkConnectionToClient conn)
    {
        if (!ConnectionInfo.ClientToScene.TryGetValue(conn, out var sceneType))
        {
            return;
        }
        
        SceneManager.MoveGameObjectToScene(entity, SceneConfig.GetSceneToType(sceneType));
    }

    private void RegisterPlayerForConnection(NetworkConnectionToClient conn, GameObject playerObject)
    {
        NetworkServer.AddPlayerForConnection(conn, playerObject);
        ConnectionInfo.PlayerEntityId[conn] = playerObject.GetComponent<NetworkIdentity>();
    }

    private void RegisterObjectForConnection(NetworkConnectionToClient conn, GameObject networkObject) => NetworkServer.Spawn(networkObject, conn);

    private void SendSpawnConfirmation(NetworkConnectionToClient conn, in SyncObjectSpawn originalSpawn, NetworkIdentity identity) =>
    NetworkUtility.SendMessage(new SyncObjectSpawn(originalSpawn, identity.netId), conn);

    private void TrackClientEntity(NetworkConnectionToClient conn, NetworkIdentity netId) => ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new() { netId }).Add(netId);
}
