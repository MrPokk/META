#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SceneConfig))]
public class SceneConfigEditor : Editor
{
    private SerializedProperty sceneMappingsProp;
    
    private GUIContent syncButtonContent = new GUIContent("Sync Scene Mappings", "Automatically match scene types with scene assets");
    private GUIContent updateNamesButtonContent = new GUIContent("Update Scene Names", "Force update all scene names from scene assets");

    private void OnEnable()
    {
        sceneMappingsProp = serializedObject.FindProperty("sceneMappings");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        
        // Кнопки управления
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(syncButtonContent, GUILayout.Height(30)))
        {
            SyncSceneMappings();
            Debug.Log("Scene mappings synced with scene types");
        }
        EditorGUILayout.EndHorizontal();

        // Отступ перед списком
        EditorGUILayout.Space();

        // Отображаем маппинги сцен
        EditorGUILayout.PropertyField(sceneMappingsProp, true);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private void UpdateAllSceneNames()
    {
        for (int i = 0; i < sceneMappingsProp.arraySize; i++)
        {
            var element = sceneMappingsProp.GetArrayElementAtIndex(i);
            var sceneAssetProp = element.FindPropertyRelative("sceneAsset");
            var sceneNameProp = element.FindPropertyRelative("_sceneName");
            
            if (sceneAssetProp.objectReferenceValue != null)
            {
                sceneNameProp.stringValue = sceneAssetProp.objectReferenceValue.name;
            }
        }
    }

    private void SyncSceneMappings()
    {
        var allScenePaths = AssetDatabase.FindAssets("t:SceneAsset")
                               .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                               .ToList();

        var sceneTypes = System.Enum.GetValues(typeof(SceneTypes)).Cast<SceneTypes>().ToList();
        var newMappings = new List<SceneMappingData>();

        foreach (var sceneType in sceneTypes)
        {
            var existingMapping = FindExistingMapping(sceneType);
            string searchPattern = sceneType.ToString();
            var matchingScene = FindMatchingScene(allScenePaths, searchPattern);

            newMappings.Add(new SceneMappingData {
                sceneType = sceneType,
                sceneAsset = existingMapping.sceneAsset ?? 
                           (matchingScene != null ? AssetDatabase.LoadAssetAtPath<SceneAsset>(matchingScene) : null)
            });
        }

        UpdateSceneMappingsArray(newMappings);
        UpdateBuildSettings(newMappings);
        UpdateAllSceneNames(); // Автоматически обновляем имена после синхронизации
    }

    private struct SceneMappingData
    {
        public SceneTypes sceneType;
        public SceneAsset sceneAsset;
    }

    private SceneMappingData FindExistingMapping(SceneTypes sceneType)
    {
        for (int i = 0; i < sceneMappingsProp.arraySize; i++)
        {
            var element = sceneMappingsProp.GetArrayElementAtIndex(i);
            var typeProp = element.FindPropertyRelative("sceneType");
            if ((SceneTypes)typeProp.enumValueIndex == sceneType)
            {
                return new SceneMappingData {
                    sceneType = sceneType,
                    sceneAsset = element.FindPropertyRelative("sceneAsset").objectReferenceValue as SceneAsset
                };
            }
        }
        return new SceneMappingData();
    }

    private string FindMatchingScene(List<string> allScenePaths, string searchPattern)
    {
        return allScenePaths.FirstOrDefault(p =>
            System.IO.Path.GetFileNameWithoutExtension(p).Contains(searchPattern));
    }

    private void UpdateSceneMappingsArray(List<SceneMappingData> newMappings)
    {
        sceneMappingsProp.ClearArray();
        sceneMappingsProp.arraySize = newMappings.Count;

        for (int i = 0; i < newMappings.Count; i++)
        {
            var element = sceneMappingsProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("sceneType").enumValueIndex = (int)newMappings[i].sceneType;
            element.FindPropertyRelative("sceneAsset").objectReferenceValue = newMappings[i].sceneAsset;
            element.FindPropertyRelative("_sceneName").stringValue = newMappings[i].sceneAsset != null ? 
                newMappings[i].sceneAsset.name : string.Empty;
        }
    }

    private void UpdateBuildSettings(List<SceneMappingData> mappings)
    {
        var buildScenes = new List<EditorBuildSettingsScene>();

        foreach (var mapping in mappings)
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
