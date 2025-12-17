using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;
using System;
using Object = UnityEngine.Object;
using BitterECS.Core;
using R3;

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
        if (!NetworkClient.spawned.TryGetValue(spawn.netId, out var clientGameObject))
        {
            return;
        }

        if (!clientGameObject.TryGetComponent<MonoProvider>(out var provider))
        {
            return;
        }

        provider.Entity.Add<ControllableComponent>(new());
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок синхронизации объектов на сервере
    // Включает проверки на null соединение, префаб, экземпляр и компоненты
    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        try
        {
            // Проверка на null соединение
            if (conn == null)
            {
                LoggerUtility.Error("OnServerSync called with null connection");
                return;
            }

            if (IsHavePlayerIdentity(conn)) return;

            var entityPrefab = FindEntityPrefab(spawn.entity.Type);
            // Проверка на null префаб
            if (entityPrefab == null)
            {
                LoggerUtility.Error($"Failed to find entity prefab for type: {spawn.entity.Type}");
                return;
            }

            var goInstance = CreateEntityInstance(spawn, entityPrefab, conn);
            // Проверка на null экземпляр
            if (goInstance == null)
            {
                LoggerUtility.Error("Failed to create entity instance");
                return;
            }

            MoveEntityToClientScene(goInstance, conn);

            var identity = goInstance.GetComponent<NetworkIdentity>();
            // Проверка на наличие NetworkIdentity компонента
            if (identity == null)
            {
                LoggerUtility.Error($"GameObject {goInstance.name} does not have NetworkIdentity component");
                return;
            }

            if (identity.TryGetComponent<PlayerProvider>(out var _))
            {
                LoggerUtility.Info($"Registering player for connection {conn.connectionId}");
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
        catch (Exception ex)
        {
            // Логируем ошибку синхронизации объекта
            LoggerUtility.Error($"Error in OnServerSync: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private bool IsHavePlayerIdentity(NetworkConnectionToClient conn)
    {
        if (!ConnectionInfo.PlayerEntityId.TryGetValue(conn, out _))
        {
            return false;
        }

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

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок создания экземпляра объекта
    // Включает проверки на null префаб и результат инстанцирования
    private GameObject CreateEntityInstance(in SyncObjectSpawn spawn, GameObject prefab, NetworkConnectionToClient conn)
    {
        try
        {
            // Проверка на null префаб перед инстанцированием
            if (prefab == null)
            {
                LoggerUtility.Error("Cannot instantiate null prefab");
                return null;
            }

            var instance = Object.Instantiate(prefab, spawn.position, spawn.rotation);
            // Проверка на null результат инстанцирования
            if (instance == null)
            {
                LoggerUtility.Error($"Failed to instantiate prefab: {prefab.name}");
                return null;
            }

            instance.name = $"{prefab.name} [{conn.connectionId}]";
            return instance;
        }
        catch (Exception ex)
        {
            // Логируем ошибку создания экземпляра
            LoggerUtility.Error($"Error creating entity instance: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    private void MoveEntityToClientScene(GameObject entity, NetworkConnectionToClient conn)
    {
        if (!ConnectionInfo.ClientToScene.TryGetValue(conn, out var sceneType))
        {
            return;
        }

        SceneManager.MoveGameObjectToScene(entity, SceneConfig.GetSceneToType(sceneType));
    }

    // ========== [LOGGING ADDED] ==========
    // Добавлено логирование ошибок регистрации игрока для соединения
    // Включает проверки на null соединение, объект игрока и NetworkIdentity компонент
    private void RegisterPlayerForConnection(NetworkConnectionToClient conn, GameObject playerObject)
    {
        try
        {
            // Проверка на null соединение
            if (conn == null)
            {
                LoggerUtility.Error("Cannot register player for null connection");
                return;
            }

            // Проверка на null объект игрока
            if (playerObject == null)
            {
                LoggerUtility.Error("Cannot register null player object");
                return;
            }

            NetworkServer.AddPlayerForConnection(conn, playerObject);
            var identity = playerObject.GetComponent<NetworkIdentity>();
            // Проверка на наличие NetworkIdentity компонента
            if (identity == null)
            {
                LoggerUtility.Error($"Player object {playerObject.name} does not have NetworkIdentity");
                return;
            }
            ConnectionInfo.PlayerEntityId[conn] = identity;
        }
        catch (Exception ex)
        {
            // Логируем ошибку регистрации игрока
            LoggerUtility.Error($"Error registering player for connection: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void RegisterObjectForConnection(NetworkConnectionToClient conn, GameObject networkObject) => NetworkServer.Spawn(networkObject, conn);

    private void SendSpawnConfirmation(NetworkConnectionToClient conn, in SyncObjectSpawn originalSpawn, NetworkIdentity identity) =>
    NetworkUtility.SendMessage(new SyncObjectSpawn(originalSpawn, identity.netId), conn);

    private void TrackClientEntity(NetworkConnectionToClient conn, NetworkIdentity netId) => ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new() { netId }).Add(netId);
}
