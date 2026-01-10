using System;
using Mirror;
using Cysharp.Threading.Tasks;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class NetworkUtility
{
    public static NetworkType Type { get; private set; }
    private static readonly Stack<Type> s_messages = new();

    public static NetworkType Initialize(NetworkConfig networkConfig)
    {
        s_messages.Clear();

#if !UNITY_EDITOR
        if (networkConfig.NetworkType == NetworkType.Server)
        {
            var configPath = GetServerConfigPath();
            LoadOrSaveServerConfig(networkConfig, configPath);
        }
#endif
        return Type = networkConfig.NetworkType;
    }

    public static async UniTask SendMessage<T>(T value, NetworkConnection target = null) where T : struct, NetworkMessage
    {
        if (NetworkServer.active && target != null)
        {
            target.Send(value);
        }
        else if (NetworkServer.active)
        {
            NetworkServer.SendToAll(value);
        }
        else if (NetworkClient.active)
        {
            await WaitingToSend(value);
        }
        else
        {
            LoggerUtility.Warning("Waiting for connection...");
        }
    }

    private static async UniTask WaitingToSend<T>(T message) where T : struct, NetworkMessage
    {
        try
        {
            if (s_messages.Any() && s_messages.Peek() == typeof(T))
            {
                return;
            }

            s_messages.Push(typeof(T));

            await UniTask.WaitUntil(() =>
                NetworkClient.connection != null &&
                NetworkClient.connection.isReady
            );

            NetworkClient.Send(message);

            s_messages.Pop();
        }
        catch (OperationCanceledException)
        {
            LoggerUtility.Warning("NetworkClient is not ready");
        }
        catch (Exception ex)
        {
            LoggerUtility.Critical($"Failed to send network message: {ex.Message}");
        }
    }

    public static bool IsClientActive()
    {
        if (NetworkClient.connection == null || !NetworkClient.active)
        {
            LoggerUtility.Info("NetworkClient is not active", NetworkType.Client);
            return false;
        }
        return true;
    }

    public static bool IsServerActive()
    {
        if (!NetworkServer.active)
        {
            LoggerUtility.Info("NetworkServer is not active", NetworkType.Server);
            return false;
        }
        return true;
    }

    private static void LoadOrSaveServerConfig(NetworkConfig config, string configPath)
    {
        var configDir = Path.GetDirectoryName(configPath);

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        if (File.Exists(configPath))
        {
            config.LoadFromFile(configPath);
            LoggerUtility.Info($"Loaded server config from: {configPath}", NetworkType.Server);
        }
        else
        {
            config.SaveToFile(configPath);
            LoggerUtility.Info($"Created new server config at: {configPath}", NetworkType.Server);
        }
    }

    private static string GetServerConfigPath()
    {
        var dataPath = Application.dataPath;
        var executableDir = Path.GetDirectoryName(dataPath);
        return Path.Combine(executableDir, "config", "server_config.json");
    }
}
