#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(SceneConfig))]
public class SceneConfigEditor : Editor
{
    private SceneConfig config;

    private void OnEnable()
    {
        config = (SceneConfig)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto-Fill from Build Settings"))
        {
            AutoFillFromBuildSettings();
        }

        if (GUILayout.Button("Validate Config"))
        {
            ValidateConfig();
        }
    }

    private void AutoFillFromBuildSettings()
    {
        // Get all scenes from Build Settings
        var scenesInBuild = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => System.IO.Path.GetFileNameWithoutExtension(scene.path))
            .ToList();

        // Clear existing mappings if empty
        if (config.sceneMappings == null)
        {
            config.sceneMappings = new List<SceneConfig.SceneMapping>();
        }

        // Add new scenes that don't exist in the config
        foreach (var sceneName in scenesInBuild)
        {
            bool exists = config.sceneMappings.Any(m => m.sceneName == sceneName);
            if (!exists)
            {
                var newMapping = new SceneConfig.SceneMapping
                {
                    sceneType = GetUniqueSceneType(sceneName),
                    sceneName = sceneName,
                    isLoadServer = false // Default value
                };
                config.sceneMappings.Add(newMapping);
            }
        }

        // Remove scenes that no longer exist in Build Settings
        for (int i = config.sceneMappings.Count - 1; i >= 0; i--)
        {
            if (!scenesInBuild.Contains(config.sceneMappings[i].sceneName))
            {
                config.sceneMappings.RemoveAt(i);
            }
        }

        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"SceneConfig updated with {config.sceneMappings.Count} scenes from Build Settings");
    }

    private SceneTypes GetUniqueSceneType(string sceneName)
    {
        // Try to find existing enum value that matches the scene name
        if (System.Enum.TryParse<SceneTypes>(sceneName, true, out var existingType))
        {
            return existingType;
        }

        // Generate a new unique value by incrementing
        if (config.sceneMappings.Count > 0)
        {
            var maxValue = config.sceneMappings.Max(m => (int)m.sceneType);
            return (SceneTypes)(maxValue + 1);
        }

        return SceneTypes.EntryPoint; // Default fallback
    }

    private void ValidateConfig()
    {
        bool hasErrors = false;

        // Check for duplicate scene names
        var duplicateNames = config.sceneMappings
            .GroupBy(m => m.sceneName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count > 0)
        {
            hasErrors = true;
            Debug.LogError($"Duplicate scene names found: {string.Join(", ", duplicateNames)}");
        }

        // Check for duplicate scene types
        var duplicateTypes = config.sceneMappings
            .GroupBy(m => m.sceneType)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateTypes.Count > 0)
        {
            hasErrors = true;
            Debug.LogError($"Duplicate scene types found: {string.Join(", ", duplicateTypes)}");
        }

        // Check for empty scene names
        var emptyNames = config.sceneMappings
            .Where(m => string.IsNullOrEmpty(m.sceneName))
            .ToList();

        if (emptyNames.Count > 0)
        {
            hasErrors = true;
            Debug.LogError($"Empty scene names found in {emptyNames.Count} mappings");
        }

        if (!hasErrors)
        {
            Debug.Log("SceneConfig validation successful - no duplicates found");
        }
    }
}
#endif
