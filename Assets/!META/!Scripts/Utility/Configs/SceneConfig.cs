using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneConfig", menuName = "Configs/SceneConfig")]
public class SceneConfig : ScriptableObject
{
    [Serializable]
    public struct SceneMapping
    {
        public SceneTypes sceneType;
#if UNITY_EDITOR
        public UnityEditor.SceneAsset sceneAsset;
#endif
        public string sceneName;
    }

    public SceneMapping[] sceneMappings;

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
}
