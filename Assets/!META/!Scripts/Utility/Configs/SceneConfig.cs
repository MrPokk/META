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
        [Scene] public string sceneName;
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
        Debug.LogError($"Scene name for type {sceneType} not found!");
        return null;
    }

    public SceneMapping[] GetServerLoadScenes()
    {
        var serverScenes = new List<SceneMapping>();
        foreach (var mapping in sceneMappings)
        {
            if (mapping.isLoadServer)
            {
                serverScenes.Add(mapping);
            }
        }
        return serverScenes.ToArray();
    }
}
