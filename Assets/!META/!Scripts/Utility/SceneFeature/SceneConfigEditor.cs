#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mirror;

[CustomEditor(typeof(SceneConfig))]
public class SceneConfigEditor : Editor
{
    private SceneConfig _config;
    private List<SceneTypes> _requiredSceneTypes;

    private void OnEnable()
    {
        _config = (SceneConfig)target;
        UpdateRequiredSceneTypes();
    }

    private void UpdateRequiredSceneTypes()
    {
        _requiredSceneTypes = System.Enum.GetValues(typeof(SceneTypes))
            .Cast<SceneTypes>()
            .Where(t => t != SceneTypes.None)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw firstSceneToLoadClient field
        EditorGUILayout.PropertyField(serializedObject.FindProperty("firstSceneToLoadClient"));

        // Draw sceneMappings list
        EditorGUILayout.LabelField("Scene Mappings (Auto-managed)", EditorStyles.boldLabel);
        
        SyncMappingsToEnum();

        var mappingsProperty = serializedObject.FindProperty("sceneMappings");
        for (int i = 0; i < mappingsProperty.arraySize; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var element = mappingsProperty.GetArrayElementAtIndex(i);
                
                // Scene Type (read-only)
                EditorGUILayout.LabelField("Scene Type", element.FindPropertyRelative("sceneType").enumDisplayNames[element.FindPropertyRelative("sceneType").enumValueIndex]);
                
                // Scene Name
                EditorGUILayout.PropertyField(element.FindPropertyRelative("sceneName"));
                
                // Scene Path with [Scene] attribute support
                var scenePathProperty = element.FindPropertyRelative("sceneToPath");
                EditorGUILayout.PropertyField(scenePathProperty, new GUIContent("Scene Path"));
                
                // Load on Server
                EditorGUILayout.PropertyField(element.FindPropertyRelative("isLoadServer"));
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Auto-Fill from Build"))
            {
                AutoFillFromBuildSettings();
            }

            if (GUILayout.Button("Validate Config"))
            {
                ValidateConfig();
            }
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void SyncMappingsToEnum()
    {
        var mappingsProperty = serializedObject.FindProperty("sceneMappings");
        
        // Add missing mappings
        foreach (var sceneType in _requiredSceneTypes)
        {
            bool exists = false;
            for (int i = 0; i < mappingsProperty.arraySize; i++)
            {
                if ((SceneTypes)mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("sceneType").enumValueIndex == sceneType)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                mappingsProperty.arraySize++;
                var newElement = mappingsProperty.GetArrayElementAtIndex(mappingsProperty.arraySize - 1);
                newElement.FindPropertyRelative("sceneType").enumValueIndex = (int)sceneType;
                newElement.FindPropertyRelative("sceneName").stringValue = sceneType.ToString();
                newElement.FindPropertyRelative("sceneToPath").stringValue = sceneType.ToString();
                newElement.FindPropertyRelative("isLoadServer").boolValue = false;
            }
        }

        // Remove mappings that shouldn't exist
        for (int i = mappingsProperty.arraySize - 1; i >= 0; i--)
        {
            var sceneType = (SceneTypes)mappingsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("sceneType").enumValueIndex;
            if (!_requiredSceneTypes.Contains(sceneType))
            {
                mappingsProperty.DeleteArrayElementAtIndex(i);
            }
        }
    }

    private void AutoFillFromBuildSettings()
    {
        var scenesInBuild = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToList();

        var mappingsProperty = serializedObject.FindProperty("sceneMappings");
        for (int i = 0; i < mappingsProperty.arraySize; i++)
        {
            var element = mappingsProperty.GetArrayElementAtIndex(i);
            var sceneType = (SceneTypes)element.FindPropertyRelative("sceneType").enumValueIndex;
            
            var matchingScene = scenesInBuild.FirstOrDefault(s => s == sceneType.ToString());
            if (matchingScene != null)
            {
                element.FindPropertyRelative("sceneName").stringValue = matchingScene;
                element.FindPropertyRelative("sceneToPath").stringValue = matchingScene;
            }
        }

        serializedObject.ApplyModifiedProperties();
        Debug.Log($"SceneConfig updated with scenes from Build Settings");
    }

    private void ValidateConfig()
    {
        bool hasErrors = false;

        // Check that all required scene types are present
        foreach (var sceneType in _requiredSceneTypes)
        {
            if (!_config.sceneMappings.Any(m => m.sceneType == sceneType))
            {
                hasErrors = true;
                Debug.LogError($"Missing mapping for scene type: {sceneType}");
            }
        }

        // Check duplicates
        var typeGroups = _config.sceneMappings.GroupBy(m => m.sceneType);
        foreach (var group in typeGroups.Where(g => g.Count() > 1))
        {
            hasErrors = true;
            Debug.LogError($"Duplicate scene type: {group.Key}");
        }

        // Check empty names
        foreach (var mapping in _config.sceneMappings)
        {
            if (string.IsNullOrEmpty(mapping.sceneName))
            {
                hasErrors = true;
                Debug.LogError($"Empty scene name found for type: {mapping.sceneType}");
            }

            if (string.IsNullOrEmpty(mapping.sceneToPath))
            {
                hasErrors = true;
                Debug.LogError($"Empty scene path found for type: {mapping.sceneType}");
            }
        }

        if (!hasErrors)
        {
            Debug.Log("SceneConfig validation passed with no errors");
        }
    }
}
#endif
