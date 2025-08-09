using UnityEngine;
using UnityEditor;
using static NetworkObjectPrefabConfig;

[CustomEditor(typeof(NetworkObjectPrefabConfig))]
public class NetworkObjectPrefabConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var config = target as NetworkObjectPrefabConfig;

        var mappingsProperty = serializedObject.FindProperty("prefabMappings");
        EditorGUILayout.PropertyField(mappingsProperty, true);
        
        if (GUILayout.Button("Add New Prefab"))
        {
            var newMapping = new NetworkPrefabMapping();
            config.prefabMappings.Add(newMapping);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Prefab ID Class"))
        {
            config.GeneratePrefabIdClass();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
