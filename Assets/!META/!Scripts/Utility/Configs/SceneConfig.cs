using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;

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
    [field: SerializeField] public SceneTypes firstSceneToLoadClient { get; private set; }
    public List<SceneMapping> sceneMappings;

    public string StringFirstSceneToLoadClient() => GetSceneName(firstSceneToLoadClient);

    public string GetSceneName(SceneTypes sceneType)
    {
        foreach (var mapping in sceneMappings)
        {
            if (mapping.sceneType == sceneType)
                return mapping.sceneName;
        }

        LoggerUtility.Error($"Scene name for type {sceneType} not found!");
        return null;
    }

    public List<SceneMapping> GetServerLoadScenes()
    {
        var serverScenes = new List<SceneMapping>();
        foreach (var mapping in sceneMappings)
        {
            if (mapping.isLoadServer)
            {
                serverScenes.Add(mapping);
            }
        }

        if (serverScenes.Count == 0)
        {
            LoggerUtility.Error("No server load scenes found!");
        }
        
        return serverScenes;
    }

    public bool ValidateScene(SceneTypes sceneType, Predicate<SceneMapping> predicate = null)
    {
        if (sceneType == SceneTypes.None) return false;

        foreach (var mapping in sceneMappings)
        {
            if (mapping.sceneType == sceneType && (predicate == null || predicate(mapping)))
            {
                return true;
            }
        }

        LoggerUtility.Error($"Invalid scene type: {sceneType}");
        return false;
    }
}
