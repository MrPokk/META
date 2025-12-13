using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneNetworkProvider : IProviderHandler
{
    public static void ChangeScene(SceneTypes sceneType) => NetworkUtility.SendMessage<SceneChangeRequestMessage>(new(sceneType));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<SceneChangeRequestMessage>(OnClientRequest);
    }

    private async void OnClientRequest(SceneChangeRequestMessage message)
    {
        await SceneLoader.LoadSceneAsync(message.sceneType, () => { NetworkUtility.SendMessage(new SceneTransitionCompleteMessage()); });
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<SceneChangeRequestMessage>(OnServerRequest);
        NetworkServer.RegisterHandler<SceneTransitionCompleteMessage>(OnServerTransitionComplete);
    }

    private void OnServerRequest(NetworkConnectionToClient client, SceneChangeRequestMessage message)
    {
        if (!SceneConfig.IsServerScene(message.sceneType))
        {
            LoggerUtility.Error($"Scene {message.sceneType} is not a server scene!");
            return;
        }

        ConnectionInfo.ClientToScene[client] = message.sceneType;
        ConnectionInfo.SceneToConnections.GetOrAdd(message.sceneType, _ => new() { client }).Add(client);

        client.Send(new SceneChangeRequestMessage(message.sceneType));
    }

    private void OnServerTransitionComplete(NetworkConnectionToClient client, SceneTransitionCompleteMessage message)
    {
        if (ConnectionInfo.ClientToScene.TryGetValue(client, out var sceneType))
        {
            MoveClientObjectsToScene(client, sceneType);
        }
        else
        {
            LoggerUtility.Error($"No scene type found for connection {client.connectionId}");
        }
    }

    private void MoveClientObjectsToScene(NetworkConnectionToClient client, SceneTypes sceneType)
    {
        var scene = SceneConfig.GetSceneToType(sceneType);
        if (!scene.IsValid())
        {
            LoggerUtility.Error($"Scene {sceneType} is not valid!");
            return;
        }

        if (!ConnectionInfo.PlayerEntityId.TryGetValue(client, out var playerEntity))
        {
            LoggerUtility.Warning($"No player entity id found for connection {client.connectionId}");
            return;
        }

        NetworkServer.RemovePlayerForConnection(client, RemovePlayerOptions.Unspawn);
        SceneManager.MoveGameObjectToScene(playerEntity.gameObject, scene);
        SetPositionPlayerToSpawnPoint(playerEntity, scene);
        NetworkServer.AddPlayerForConnection(client, playerEntity.gameObject);

        SyncObjectSpawn(client, new SyncObjectSpawn(playerEntity.netId));
    }

    private static void SetPositionPlayerToSpawnPoint(NetworkIdentity player, Scene scene)
    {
        EntryPointFloors entryPoint = null;

        var rootGameObjects = scene.GetRootGameObjects();
        foreach (var gameObject in rootGameObjects)
        {
            if (gameObject.TryGetComponent(out EntryPointFloors component))
            {
                entryPoint = component;
                break;
            }
        }

        if (entryPoint == null)
        {
            LoggerUtility.Error($"No entry point found in scene {scene.name}");
            return; // TODO make disconnect
        }

        player.transform.position = FindPositionToSpawn(player, entryPoint);
    }

    private static Vector3 FindPositionToSpawn(NetworkIdentity player, EntryPointFloors entryPoint)
    {
        Vector3 position;
        var rayOrigin = entryPoint.PlayerSpawnPoint;
        var ray = new Ray(rayOrigin, Vector3.down);

        var layerMask = ~(1 << 2); // Все слои кроме IgnoreRaycast

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
        {
            var playerController = player.GetComponent<CharacterController>();

            var playerHeight = playerController.height;
            var playerCenter = playerController.center;

            position = hit.point;
            position.y -= playerCenter.y;
            position.y += playerHeight / 2f;

            position.y += 0.1f;
        }
        else
        {
            LoggerUtility.Error($"No floor found below entry point at {rayOrigin}. Using spawn point.");
            position = entryPoint.PlayerSpawnPoint;
        }

        return position;
    }

    private void SyncObjectSpawn(NetworkConnectionToClient client, SyncObjectSpawn spawn) => NetworkUtility.SendMessage(spawn, client);
}

