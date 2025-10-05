#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
[CustomEditor(typeof(TeleportPresenter))]
public class TeleportPresenterEditor : Editor
{
    private readonly Dictionary<TeleportView, bool> _foldoutStates = new();
    public override void OnInspectorGUI()
    {
        TeleportPresenter teleportPresenter = (TeleportPresenter)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Add Teleport"))
        {
            teleportPresenter.CreateTeleport();
            EditorUtility.SetDirty(teleportPresenter);
        }
        EditorGUILayout.LabelField("Teleports", EditorStyles.boldLabel);

        foreach (var valuePair in teleportPresenter.GetTeleports())
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.ObjectField(valuePair.Key, typeof(TeleportView), true);

            _foldoutStates.TryAdd(valuePair.Key, false);

            _foldoutStates[valuePair.Key] = EditorGUILayout.Foldout(_foldoutStates[valuePair.Key], "Transform");

            if (_foldoutStates[valuePair.Key])
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);

                valuePair.Key.transform.position = EditorGUILayout.Vector3Field("Position", valuePair.Key.transform.position);
                valuePair.Key.transform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", valuePair.Key.transform.rotation.eulerAngles));
                valuePair.Key.transform.localScale = EditorGUILayout.Vector3Field("Scale", valuePair.Key.transform.localScale);
                EditorGUILayout.EndVertical();
            }

            valuePair.Key.floorNumber = EditorGUILayout.IntField("Number Floor", valuePair.Key.floorNumber);
            if (valuePair.Key.floorNumber < 0)
                valuePair.Key.floorNumber = 0;

            valuePair.Key.scaleFactor = EditorGUILayout.FloatField("Scale Factor", valuePair.Key.scaleFactor);
            if (valuePair.Key.scaleFactor < 0)
                valuePair.Key.scaleFactor = 1;

            if (GUILayout.Button("Remove Teleport", GUILayout.Width(120)))
            {
                teleportPresenter.AllTeleport.Remove(valuePair.Key);
                DestroyImmediate(valuePair.Key.gameObject);
                EditorUtility.SetDirty(teleportPresenter);
            }

            EditorGUILayout.EndVertical();
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(teleportPresenter);
        }
    }
}
#endif
