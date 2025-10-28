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
        NetworkServer.RegisterHandler<SyncObjectSpawn>(OnServerSync);
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        if (NetworkClient.spawned.TryGetValue(spawn.assetId, out var clientGameObject))
        {
            if (clientGameObject.TryGetComponent<MonoProvider>(out var provider))
            {
                provider.Entity.Add<ControllableComponent>(new());// Убрать это
            }
        }
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        var typeEntity = spawn.entity.Type;
        var spawnToPrefab = NetworkManager.singleton.spawnPrefabs;
        var entityPrefab = spawnToPrefab.Find(e => e.gameObject.TryGetComponent(typeEntity, out var entity));

        if (entityPrefab == null)
        {
            LoggerUtility.Error($"Prefab with component {typeEntity} not found in spawnPrefabs");
            return;
        }


        var goInstance = Object.Instantiate(entityPrefab.gameObject);
        goInstance.name = $"{entityPrefab.gameObject.name} [{conn.connectionId}]";

        if (ConnectionInfo.ClientToScene.TryGetValue(conn, out var sceneType))
        {
            var scene = SceneConfig.GetSceneToType(sceneType);
            SceneManager.MoveGameObjectToScene(goInstance, scene);
        }

        var identity = goInstance.GetComponent<NetworkIdentity>();

        NetworkServer.Spawn(goInstance, conn);
        NetworkServer.AddPlayerForConnection(conn, goInstance);

        conn.Send(new SyncObjectSpawn(spawn, identity.netId));

        ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new()).Add(identity.netId);
    }
}
