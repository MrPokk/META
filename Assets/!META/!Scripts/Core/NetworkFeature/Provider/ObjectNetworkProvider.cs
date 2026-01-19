using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using System;
using Object = UnityEngine.Object;
using Cysharp.Threading.Tasks;

public class ObjectNetworkProvider : IProviderHandler
{
    public static async UniTask Spawn<T>(Vector3 position, Quaternion rotation) where T : MonoProvider =>
    await NetworkUtility.SendMessage(new SyncObjectSpawn(typeof(T), position, rotation));

    public static async UniTask Destroy(uint netId) =>
    await NetworkUtility.SendMessage(new DestroyObjectRequestMessage(netId));

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
        if (!NetworkClient.spawned.TryGetValue(spawn.netId, out var clientGameObject)) return;
        if (!clientGameObject.TryGetComponent<MonoProvider>(out var provider)) return;

        provider.transform.SetPositionAndRotation(spawn.position, spawn.rotation);

        provider.Entity.Add<ControllableComponent>(new());
        provider.Entity.Add<CameraEventComponent>(new());
    }

    private void OnClientDestroy(DestroyObjectRequestMessage destroyMessage)
    {
        if (!NetworkClient.spawned.TryGetValue(destroyMessage.netId, out var networkObject)) return;

        if (networkObject.TryGetComponent<MonoProvider>(out var provider))
        {
            provider.Entity?.Dispose();
        }

        NetworkClient.spawned.Remove(destroyMessage.netId);
        Object.Destroy(networkObject.gameObject);

        LoggerUtility.Info($"Client destroyed object with netId: {destroyMessage.netId}", NetworkType.Client);
    }

    private async void OnServerSync(NetworkConnectionToClient connection, SyncObjectSpawn spawn)
    {
        if (HasPlayerIdentity(connection)) return;

        var entityPrefab = FindEntityPrefab(spawn.entity.Type);
        if (entityPrefab == null) return;

        var instance = CreateEntityInstance(spawn, entityPrefab, connection);

        if (instance.TryGetComponent<PlayerProvider>(out var _))
        {
            LoggerUtility.Info($"Registering player for connection {connection}", NetworkType.Server);
            MovePlayerToClientScene(instance, connection, ref spawn);
            RegisterPlayerForConnection(connection, instance);
        }
        else
        {
            LoggerUtility.Info("Registering object for connection", NetworkType.Server);
            MoveEntityToClientScene(instance, connection);
            RegisterObjectForConnection(connection, instance);
        }

        await SendSpawnConfirmation(connection, spawn, instance);
        TrackClientEntity(connection, instance);
    }

    private async void OnServerDestroy(NetworkConnectionToClient connection, DestroyObjectRequestMessage destroyMessage)
    {
        if (!NetworkServer.spawned.TryGetValue(destroyMessage.netId, out var networkIdentity))
        {
            LoggerUtility.Warning($"Object with netId {destroyMessage.netId} not found on server", NetworkType.Server);
            return;
        }

        if (!ConnectionInfo.ClientEntities.TryGetValue(connection, out var clientObjects) || !clientObjects.Contains(networkIdentity))
        {
            LoggerUtility.Warning($"Connection {connection.connectionId} does not own object with netId {destroyMessage.netId}", NetworkType.Server);
            return;
        }

        if (ConnectionInfo.PlayerEntityId.TryGetValue(connection, out var playerIdentity) && playerIdentity.netId == destroyMessage.netId)
        {
            ConnectionInfo.PlayerEntityId.Remove(connection);
        }

        clientObjects.Remove(networkIdentity);

        await NetworkUtility.SendMessage(new DestroyObjectRequestMessage(destroyMessage.netId));
        NetworkServer.Destroy(networkIdentity.gameObject);

        LoggerUtility.Info($"Server destroyed object with netId: {destroyMessage.netId} for connection {connection.connectionId}", NetworkType.Server);
    }

    private bool HasPlayerIdentity(NetworkConnectionToClient connection) => ConnectionInfo.PlayerEntityId.ContainsKey(connection);

    private GameObject FindEntityPrefab(Type entityType)
    {
        var spawnPrefabs = NetworkManager.singleton.spawnPrefabs;
        var entityPrefab = spawnPrefabs.Find(prefab => prefab.TryGetComponent(entityType, out _));

        if (entityPrefab == null)
        {
            LoggerUtility.Error($"Prefab with component {entityType} not found in spawnPrefabs", NetworkType.Server);
            return null;
        }

        return entityPrefab;
    }

    private NetworkIdentity CreateEntityInstance(SyncObjectSpawn spawn, GameObject prefab, NetworkConnectionToClient connection)
    {
        var instance = Object.Instantiate(prefab, spawn.position, spawn.rotation);
        instance.name = $"{prefab.name} [{connection.connectionId}]";
        return instance.GetComponent<NetworkIdentity>();
    }

    private void MoveEntityToClientScene(NetworkIdentity entity, NetworkConnectionToClient connection)
    {
        GetSceneToMoveObject(entity, connection, out _);
    }

    private void MovePlayerToClientScene(NetworkIdentity entity, NetworkConnectionToClient connection, ref SyncObjectSpawn spawn)
    {
        var isValidScene = GetSceneToMoveObject(entity, connection, out var scene);
        if (!isValidScene)
        {
            return;
        }

        IsPlayerSpawnPoint.SetPositionPlayerToSpawnPoint(entity, scene, out var position, out var rotation);
        spawn.position = position;
        spawn.rotation = rotation;
    }

    private static bool GetSceneToMoveObject(NetworkIdentity entity, NetworkConnectionToClient connection, out Scene scene)
    {
        if (!ConnectionInfo.ClientToScene.TryGetValue(connection, out var sceneType))
        {
            scene = default;
            return false;
        }

        scene = SceneConfig.GetSceneToType(sceneType);
        SceneManager.MoveGameObjectToScene(entity.gameObject, scene);
        return true;
    }

    private void RegisterPlayerForConnection(NetworkConnectionToClient connection, NetworkIdentity playerObject)
    {
        NetworkServer.AddPlayerForConnection(connection, playerObject.gameObject);
        ConnectionInfo.PlayerEntityId[connection] = playerObject;
    }

    private void RegisterObjectForConnection(NetworkConnectionToClient connection, NetworkIdentity networkObject)
    {
        NetworkServer.Spawn(networkObject.gameObject, connection);
        ConnectionInfo.ClientEntities.GetOrAdd(connection, _ => new()).Add(networkObject);
    }

    private async UniTask SendSpawnConfirmation(NetworkConnectionToClient connection, SyncObjectSpawn originalSpawn, NetworkIdentity identity) => await NetworkUtility.SendMessage(new SyncObjectSpawn(originalSpawn, identity.netId), connection);

    private void TrackClientEntity(NetworkConnectionToClient connection, NetworkIdentity netId) => ConnectionInfo.ClientEntities.GetOrAdd(connection, _ => new()).Add(netId);
}
