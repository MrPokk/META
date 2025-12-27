using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneConfig", menuName = "Configs/SceneConfig")]
public class SceneConfig : ScriptableObject
{
    [Serializable]
    public struct SceneMapping
    {
        public SceneTypes sceneType;
        [Scene] public string sceneToPath;
        public string sceneName;
        public bool isLoadServer;
    }

    public SceneTypes firstSceneToLoadClient;

    public List<SceneMapping> sceneMappings = new();

    // Static access
    private static readonly Dictionary<SceneTypes, SceneMapping> _sceneMap = new();
    private static readonly HashSet<SceneTypes> _serverScenes = new();

    public static IReadOnlyDictionary<SceneTypes, SceneMapping> SceneMap => _sceneMap;
    public static IReadOnlyCollection<SceneTypes> ServerScenes => _serverScenes;

    private void OnEnable()
    {
        InitializeStaticData();
    }

    private void InitializeStaticData()
    {
        _sceneMap.Clear();
        _serverScenes.Clear();

        foreach (var mapping in sceneMappings)
        {
            _sceneMap[mapping.sceneType] = mapping;

            if (mapping.isLoadServer)
            {
                _serverScenes.Add(mapping.sceneType);
            }
        }

        if (_serverScenes.Count == 0)
        {
            LoggerUtility.Error("No server load scenes found!", NetworkType.Server);
        }
    }

    public static string GetSceneName(SceneTypes sceneType)
    {
        if (_sceneMap.TryGetValue(sceneType, out var mapping))
        {
            return mapping.sceneName;
        }

        LoggerUtility.Error($"Scene name for type {sceneType} not found!");
        return null;
    }

    public static Scene GetSceneToType(SceneTypes sceneType) => SceneManager.GetSceneByName(GetSceneName(sceneType));

    public static bool IsServerScene(SceneTypes sceneType) => _serverScenes.Contains(sceneType);

    public static bool TryGetMapping(SceneTypes sceneType, out SceneMapping mapping) =>
        _sceneMap.TryGetValue(sceneType, out mapping);

    public static bool ValidateScene(SceneTypes sceneType, Predicate<SceneMapping> predicate = null)
    {
        if (sceneType == SceneTypes.None) return false;

        if (!TryGetMapping(sceneType, out var mapping))
        {
            LoggerUtility.Error($"Invalid scene type: {sceneType}");
            return false;
        }

        return predicate == null || predicate(mapping);
    }

    public IReadOnlyCollection<SceneTypes> GetServerLoadScenes() => _serverScenes;

    public string StringFirstSceneToLoadClient() => GetSceneName(firstSceneToLoadClient);
}
