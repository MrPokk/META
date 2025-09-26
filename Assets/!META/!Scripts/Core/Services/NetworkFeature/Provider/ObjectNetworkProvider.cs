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

        //foreach (var entity in entities)
        //{
        //    var view = EcsLinker.GetView<EcsNetworkView>(entity);
        //    if (view == null)
        //        continue;

        //    if (!view.TryGetComponent<NetworkIdentity>(out var identity))
        //        continue;

        //    SceneManager.MoveGameObjectToScene(identity.gameObject, scene);
        //}
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        _ = NetworkClient.spawned[spawn.assetId];
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (!TryGetClientScene(conn, out var scene))
            return;


        // TODO: Порефактори для оптимизации
        var typeEntity = spawn.entity.Type;
        var spawnToPrefab = NetworkManager.singleton.spawnPrefabs;
        var entityToSpawn = spawnToPrefab.Find(e => e.gameObject.TryGetComponent(typeEntity, out var entity));

        ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new()).Add(entityToSpawn);

        var go = entityToSpawn.gameObject;
        go.name = $"{go.name} [{conn.connectionId}]";
        SceneManager.MoveGameObjectToScene(go, scene);

        var identity = go.GetComponent<NetworkIdentity>();
        NetworkServer.AddPlayerForConnection(conn, go);

        conn.Send(new SyncObjectSpawn(spawn, identity.netId));
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
