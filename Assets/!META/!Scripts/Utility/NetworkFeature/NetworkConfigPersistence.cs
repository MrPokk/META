using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class NetworkConfigPersistence
{
    private const string ConfigPath = "config";
    private const string ConfigFileName = "server_config.json";

    public static void LoadOrSaveServerConfig(NetworkConfig config)
    {
        var configPath = GetServerConfigPath();
        var configDir = Path.GetDirectoryName(configPath);

        if (!Directory.Exists(configDir))
            Directory.CreateDirectory(configDir!);

        if (File.Exists(configPath))
        {
            LoadFromFile(config, configPath);
            LoggerUtility.Info($"Loaded server config from: {configPath}", NetworkType.Server);
        }
        else
        {
            SaveToFile(config, configPath);
            LoggerUtility.Info($"Created new server config at: {configPath}", NetworkType.Server);
        }
    }

    public static void SaveToFile(NetworkConfig config, string filePath)
    {
        LoggerUtility.Info($"Saving network config to {filePath}", NetworkType.Server);

        var configData = new NetworkConfigData
        {
            commonSettings = config.Common,
            kcpConfig = config.KcpConfig,
            webSocketConfig = config.WebSocketConfig,
        };

        var json = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(NetworkConfig config, string filePath)
    {
        if (!File.Exists(filePath)) return;

        var json = File.ReadAllText(filePath);
        var configData = JsonConvert.DeserializeObject<NetworkConfigData>(json);

        config.UpdateSettings(configData);
    }

    private static string GetServerConfigPath()
    {
        var rootDir = Path.GetDirectoryName(Application.dataPath);
        return Path.Combine(rootDir!, ConfigPath, ConfigFileName);
    }
}
