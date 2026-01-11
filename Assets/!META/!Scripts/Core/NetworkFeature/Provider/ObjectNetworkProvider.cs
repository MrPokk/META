using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using System;
using Object = UnityEngine.Object;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

public class ObjectNetworkProvider : IProviderHandler
{
    public static UniTask Spawn<T>(Vector3 position, Quaternion rotation) where T : MonoProvider =>
        NetworkUtility.SendMessage(new SyncObjectSpawn(typeof(T), position, rotation));

    public static UniTask Destroy(uint netId) =>
      NetworkUtility.SendMessage(new DestroyObjectRequestMessage(netId));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SyncObjectSpawn>(OnClientSync);
        NetworkClient.RegisterHandler<DestroyObjectRequestMessage>(OnClientDestroy);
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SyncObjectSpawn>(OnServerSync);
        NetworkServer.RegisterHandler<DestroyObjectRequestMessage>(OnServerDestroy);
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        if (!NetworkClient.spawned.TryGetValue(spawn.netId, out var clientGameObject))
        {
            return;
        }

        if (!clientGameObject.TryGetComponent<MonoProvider>(out var provider))
        {
            return;
        }

        provider.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        provider.Entity.Add<ControllableComponent>(new());
        provider.Entity.Add<CameraEventComponent>(new());
    }

    private void OnClientDestroy(DestroyObjectRequestMessage destroyMessage)
    {
        if (NetworkClient.spawned.TryGetValue(destroyMessage.netId, out var networkObject))
        {
            if (networkObject.TryGetComponent<MonoProvider>(out var provider))
            {
                provider.Entity?.Dispose();
            }

            NetworkClient.spawned.Remove(destroyMessage.netId);
            Object.Destroy(networkObject.gameObject);

            LoggerUtility.Info($"Client destroyed object with netId: {destroyMessage.netId}", NetworkType.Client);
        }
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (IsHavePlayerIdentity(conn)) return;

        var entityPrefab = FindEntityPrefab(spawn.entity.Type);
        if (entityPrefab == null) return;

        var goInstance = CreateEntityInstance(spawn, entityPrefab, conn);
        MoveEntityToClientScene(goInstance, conn);

        var identity = goInstance.GetComponent<NetworkIdentity>();

        if (identity.TryGetComponent<PlayerProvider>(out var _))
        {
            LoggerUtility.Info($"Registering player for connection {conn}", NetworkType.Server);
            RegisterPlayerForConnection(conn, goInstance);
        }
        else
        {
            LoggerUtility.Info("Registering object for connection", NetworkType.Server);
            RegisterObjectForConnection(conn, goInstance);
        }

        SendSpawnConfirmation(conn, spawn, identity);
        TrackClientEntity(conn, identity);
    }

    private async void OnServerDestroy(NetworkConnectionToClient conn, DestroyObjectRequestMessage destroyMessage)
    {
        if (!NetworkServer.spawned.TryGetValue(destroyMessage.netId, out var networkIdentity))
        {
            LoggerUtility.Warning($"Object with netId {destroyMessage.netId} not found on server", NetworkType.Server);
            return;
        }

        if (!ConnectionInfo.ClientEntities.TryGetValue(conn, out var clientObjects) ||
            !clientObjects.Contains(networkIdentity))
        {
            LoggerUtility.Warning($"Connection {conn.connectionId} does not own object with netId {destroyMessage.netId}", NetworkType.Server);
            return;
        }

        if (ConnectionInfo.PlayerEntityId.TryGetValue(conn, out var playerIdentity) &&
            playerIdentity.netId == destroyMessage.netId)
        {
            ConnectionInfo.PlayerEntityId.Remove(conn);
        }

        clientObjects.Remove(networkIdentity);

        await NetworkUtility.SendMessage(new DestroyObjectRequestMessage(destroyMessage.netId));
        NetworkServer.Destroy(networkIdentity.gameObject);

        LoggerUtility.Info($"Server destroyed object with netId: {destroyMessage.netId} for connection {conn.connectionId}", NetworkType.Server);
    }

    private bool IsHavePlayerIdentity(NetworkConnectionToClient conn)
    {
        return ConnectionInfo.PlayerEntityId.TryGetValue(conn, out _);
    }

    private GameObject FindEntityPrefab(Type entityType)
    {
        var spawnToPrefab = NetworkManager.singleton.spawnPrefabs;
        var entityPrefab = spawnToPrefab.Find(e => e.TryGetComponent(entityType, out var entity));

        if (entityPrefab == null)
        {
            LoggerUtility.Error($"Prefab with component {entityType} not found in spawnPrefabs", NetworkType.Server);
            return null;
        }

        return entityPrefab;
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
        LoggerUtility.Info($"Register player for connection {conn.connectionId}", NetworkType.Server);
        NetworkServer.AddPlayerForConnection(conn, playerObject);
        ConnectionInfo.PlayerEntityId[conn] = playerObject.GetComponent<NetworkIdentity>();
    }

    private void RegisterObjectForConnection(NetworkConnectionToClient conn, GameObject networkObject) =>
        NetworkServer.Spawn(networkObject, conn);

    private UniTask SendSpawnConfirmation(NetworkConnectionToClient conn, in SyncObjectSpawn originalSpawn, NetworkIdentity identity) =>
        NetworkUtility.SendMessage(new SyncObjectSpawn(originalSpawn, identity.netId), conn);

    private void TrackClientEntity(NetworkConnectionToClient conn, NetworkIdentity netId) =>
        ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new() { netId }).Add(netId);
}
