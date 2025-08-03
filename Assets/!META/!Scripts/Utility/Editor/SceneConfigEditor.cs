#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SceneConfig))]
public class SceneConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        SceneConfig config = (SceneConfig)target;

        if (GUILayout.Button("Sync With Scene Types"))
        {
            SyncSceneMappings(config);
            Debug.Log("Scene mappings synced with scene types");
            EditorUtility.SetDirty(config);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SyncSceneMappings(SceneConfig config)
    {
        var allScenePaths = AssetDatabase.FindAssets("t:SceneAsset")
                               .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                               .ToList();

        var newMappings = new List<SceneConfig.SceneMapping>();
        var sceneTypes = System.Enum.GetValues(typeof(SceneTypes)).Cast<SceneTypes>().ToList();

        foreach (var sceneType in sceneTypes)
        {
            var existingMapping = config.sceneMappings.FirstOrDefault(m => m.sceneType == sceneType);
            if (existingMapping.sceneAsset != null)
            {
                newMappings.Add(existingMapping);
                continue;
            }

            string searchPattern = sceneType.ToString();
            var matchingScene = allScenePaths.FirstOrDefault(p => 
                System.IO.Path.GetFileNameWithoutExtension(p).Contains(searchPattern));

            if (matchingScene != null)
            {
                newMappings.Add(new SceneConfig.SceneMapping
                {
                    sceneType = sceneType,
                    sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(matchingScene),
                    sceneName = System.IO.Path.GetFileNameWithoutExtension(matchingScene)
                });
            }
            else
            {
                newMappings.Add(new SceneConfig.SceneMapping
                {
                    sceneType = sceneType,
                    sceneAsset = null,
                    sceneName = ""
                });
            }
        }

        var mappingsProp = serializedObject.FindProperty("sceneMappings");
        mappingsProp.ClearArray();
        mappingsProp.arraySize = newMappings.Count;

        for (int i = 0; i < newMappings.Count; i++)
        {
            var element = mappingsProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("sceneType").enumValueIndex = (int)newMappings[i].sceneType;
            element.FindPropertyRelative("sceneAsset").objectReferenceValue = newMappings[i].sceneAsset;
            element.FindPropertyRelative("sceneName").stringValue = newMappings[i].sceneName;
        }

        UpdateBuildSettings(config);
    }

    private void UpdateBuildSettings(SceneConfig config)
    {
        var buildScenes = new List<EditorBuildSettingsScene>();
        
        foreach (var mapping in config.sceneMappings)
        {
            if (mapping.sceneAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(mapping.sceneAsset);
                if (!buildScenes.Any(s => s.path == path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                }
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
    }
}
#endif
