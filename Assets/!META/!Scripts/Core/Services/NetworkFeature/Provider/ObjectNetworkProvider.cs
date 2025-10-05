using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Integration;

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
        NetworkServer.RegisterHandler<SyncStateSceneMessage>(OnSceneMoveSync);
        NetworkServer.RegisterHandler<SyncObjectSpawn>(OnServerSync);
    }

    private void OnSceneMoveSync(NetworkConnectionToClient client, SyncStateSceneMessage message)
    {
        if (!TryGetClientScene(client, out var scene))
            return;

        var entities = ConnectionInfo.ClientEntities.GetOrAdd(client, _ => new());

        foreach (var entity in entities)
        {
            if (NetworkServer.spawned.TryGetValue(entity, out var networkIdentity))
            {
                SceneManager.MoveGameObjectToScene(networkIdentity.gameObject, scene);
            }
        }
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        var clientGameObject = NetworkClient.spawned[spawn.assetId];
        if (clientGameObject.TryGetComponent<MonoProvider>(out var provider))
        {
            provider.Entity.Add<ControllableComponent>(new());
        }
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (!TryGetClientScene(conn, out var scene))
            return;

        var typeEntity = spawn.entity.Type;
        var spawnToPrefab = NetworkManager.singleton.spawnPrefabs;
        var entityPrefab = spawnToPrefab.Find(e => e.gameObject.TryGetComponent(typeEntity, out var entity));

        if (entityPrefab == null)
        {
            Debug.LogError($"Prefab with component {typeEntity} not found in spawnPrefabs");
            return;
        }

        var goInstance = Object.Instantiate(entityPrefab.gameObject);
        goInstance.name = $"{entityPrefab.gameObject.name} [{conn.connectionId}]";

        SceneManager.MoveGameObjectToScene(goInstance, scene);

        var identity = goInstance.GetComponent<NetworkIdentity>();

        NetworkServer.Spawn(goInstance, conn);
        NetworkServer.AddPlayerForConnection(conn, goInstance);

        conn.Send(new SyncObjectSpawn(spawn, identity.netId));

        ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new()).Add(identity.netId);
    }

    private bool TryGetClientScene(NetworkConnectionToClient conn, out Scene scene)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(conn, out var sceneType))
        {
            scene = SceneManager.GetSceneByName(SceneConfig.GetSceneName(sceneType));
            return true;
        }

        LoggerUtility.Error($"Scene for client {conn.connectionId} not found!");
        scene = default;
        return false;
    }
}
