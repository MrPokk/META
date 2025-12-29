using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IsPlayerSpawnPoint : MonoBehaviour
{
    public Vector3 Position => transform.position;

    [Server]
    public static void SetPositionPlayerToSpawnPoint(NetworkIdentity player, Scene scene, out Vector3 position, out Quaternion rotation)
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
            LoggerUtility.Error($"No entry point found in scene {scene.name}", NetworkType.Server);
            throw new Exception("No entry point found in scene");
        }

        var positionToPlayer = FindPositionToSpawn(player, entryPoint);
        var rotationToPlayer = FindRotationToSpawn(entryPoint);

        player.transform.SetPositionAndRotation(
            positionToPlayer,
            rotationToPlayer);

        position = positionToPlayer;
        rotation = rotationToPlayer;
    }

    [Server]
    private static Quaternion FindRotationToSpawn(EntryPointFloors entryPoint)
    {
        return Quaternion.LookRotation(entryPoint.PlayerSpawnRotationForward);
    }

    [Server]
    private static Vector3 FindPositionToSpawn(NetworkIdentity player, EntryPointFloors entryPoint)
    {
        Vector3 position;
        var rayOrigin = entryPoint.PlayerSpawnPoint;
        var ray = new Ray(rayOrigin, Vector3.down);

        var layerMask = LayerMask.GetMask(
            "Ignore Raycast",
            "Player");

        if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~layerMask))
        {
            position = GetPosition(player, hit);
            LoggerUtility.Info($"Floor found below entry point at {position}.", NetworkType.Server);
        }
        else
        {
            position = entryPoint.PlayerSpawnPoint;
            LoggerUtility.Error($"No floor found below entry point at {rayOrigin}. Using spawn point.", NetworkType.Server);
        }

        return position;
    }

    [Server]
    private static Vector3 GetPosition(NetworkIdentity player, RaycastHit hit)
    {
        Vector3 position;
        var playerController = player.GetComponent<CharacterController>();

        var playerHeight = playerController.height;
        var playerCenter = playerController.center;

        position = hit.point;
        position.y -= playerCenter.y;
        position.y += playerHeight / 2f;

        position.y += 0.1f;
        return position;
    }
}
