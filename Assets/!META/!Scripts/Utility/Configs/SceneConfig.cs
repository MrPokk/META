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
        [SerializeField] public UnityEditor.SceneAsset sceneAsset;
#endif
        [SerializeField] private string _sceneName;

        public string SceneName
        {
            get
            {
#if UNITY_EDITOR
                if (sceneAsset != null) return sceneAsset.name;
#endif
                return _sceneName;
            }
        }

#if UNITY_EDITOR
        public UnityEditor.SceneAsset SceneAsset => sceneAsset;
#endif
    }

    public SceneMapping[] sceneMappings;

    public string GetSceneName(SceneTypes sceneType)
    {
        foreach (var mapping in sceneMappings)
        {
            if (mapping.sceneType == sceneType)
                return mapping.SceneName;
        }
        Debug.LogError($"Scene name for type {sceneType} not found!");
        return null;
    }
}
