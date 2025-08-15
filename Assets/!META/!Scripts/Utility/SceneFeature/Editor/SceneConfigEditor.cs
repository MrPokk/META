#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(SceneConfig))]
public class SceneConfigEditor : Editor
{
    private SceneConfig _config;

    private void OnEnable()
    {
        _config = (SceneConfig)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw firstSceneToLoadClient field directly
        _config.firstSceneToLoadClient = (SceneTypes)EditorGUILayout.EnumPopup(
            new GUIContent("First Scene To Load Client"), 
            _config.firstSceneToLoadClient);

        // Draw sceneMappings list
        EditorGUILayout.LabelField("Scene Mappings", EditorStyles.boldLabel);
        
        for (int i = 0; i < _config.sceneMappings.Count; i++)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var mapping = _config.sceneMappings[i];
                
                mapping.sceneType = (SceneTypes)EditorGUILayout.EnumPopup("Scene Type", mapping.sceneType);
                mapping.sceneName = EditorGUILayout.TextField("Scene Name", mapping.sceneName);
                mapping.sceneToPath = EditorGUILayout.TextField("Scene Path", mapping.sceneToPath);
                mapping.isLoadServer = EditorGUILayout.Toggle("Load on Server", mapping.isLoadServer);
                
                _config.sceneMappings[i] = mapping;

                if (GUILayout.Button("Remove"))
                {
                    _config.sceneMappings.RemoveAt(i);
                    break;
                }
            }
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("Add New Mapping"))
        {
            _config.sceneMappings.Add(new SceneConfig.SceneMapping());
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

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
        }
    }

    private void AutoFillFromBuildSettings()
    {
        var scenesInBuild = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToList();

        _config.sceneMappings.Clear();

        foreach (var sceneName in scenesInBuild)
        {
            _config.sceneMappings.Add(new SceneConfig.SceneMapping
            {
                sceneName = sceneName,
                sceneToPath = sceneName,
                sceneType = GetUniqueSceneType(sceneName),
                isLoadServer = false
            });
        }

        EditorUtility.SetDirty(_config);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"SceneConfig updated with {scenesInBuild.Count} scenes from Build Settings");
    }

    private SceneTypes GetUniqueSceneType(string sceneName)
    {
        // Try to match existing enum values first
        if (System.Enum.TryParse<SceneTypes>(sceneName, true, out var existingType))
        {
            return existingType;
        }

        // Find next available value
        var highestValue = _config.sceneMappings
            .Select(m => (int)m.sceneType)
            .DefaultIfEmpty()
            .Max();

        return (SceneTypes)(highestValue + 1);
    }

    private void ValidateConfig()
    {
        bool hasErrors = false;

        // Check duplicates
        var nameGroups = _config.sceneMappings.GroupBy(m => m.sceneName);
        var typeGroups = _config.sceneMappings.GroupBy(m => m.sceneType);

        foreach (var group in nameGroups.Where(g => g.Count() > 1))
        {
            hasErrors = true;
            Debug.LogError($"Duplicate scene name: {group.Key}");
        }

        foreach (var group in typeGroups.Where(g => g.Count() > 1))
        {
            hasErrors = true;
            Debug.LogError($"Duplicate scene type: {group.Key}");
        }

        // Check empty names
        foreach (var mapping in _config.sceneMappings.Where(m => string.IsNullOrEmpty(m.sceneName)))
        {
            hasErrors = true;
            Debug.LogError($"Empty scene name found for type: {mapping.sceneType}");
        }

        if (!hasErrors)
        {
            Debug.Log("SceneConfig validation passed with no errors");
        }
    }
}
#endif
