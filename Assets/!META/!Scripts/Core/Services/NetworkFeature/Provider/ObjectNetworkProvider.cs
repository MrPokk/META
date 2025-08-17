using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using BitterECS.Core;
using BitterECS.Core.Integration;
using System;

public class ObjectNetworkProvider : IProviderHandler
{
    public static void Spawn<TEntity, TView>(Vector3 position, Quaternion rotation)
        => NetworkUtility.SendMessage(new SyncObjectSpawn(typeof(TEntity), typeof(TView), position, rotation));

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
            var view = EcsLinker.GetView<EcsNetworkView>(entity);
            if (view == null)
                continue;

            if (!view.TryGetComponent<NetworkIdentity>(out var identity))
                continue;

            SceneManager.MoveGameObjectToScene(identity.gameObject, scene);
        }
    }

    private void OnClientSync(SyncObjectSpawn spawn)
    {
        var typeEntity = spawn.entity.Type;

        var view = NetworkClient.spawned[spawn.assetId];
        var entity = EcsWorld.GetToEntityType(typeEntity).AddEntity(typeEntity);
        EcsLinker.Link(entity, view.GetComponent<ILinkableView>());
    }

    private void OnServerSync(NetworkConnectionToClient conn, SyncObjectSpawn spawn)
    {
        if (!TryGetClientScene(conn, out var scene))
            return;

        var typeEntity = spawn.entity.Type;
        var typeView = spawn.view.Type;

        var instance = EcsUnityViewDatabase.GetInstance(typeView, spawn.position, spawn.rotation);
        var entity = EcsWorld.GetToEntityType(typeEntity).AddEntity(typeEntity);
        EcsLinker.Link(entity, instance.linkableView);

        ConnectionInfo.ClientEntities.GetOrAdd(conn, _ => new()).Add(entity);

        var go = instance.monoBehaviour.gameObject;
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
