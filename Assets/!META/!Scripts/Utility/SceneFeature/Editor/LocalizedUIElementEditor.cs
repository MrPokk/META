#if UNITY_EDITOR
using Gley.Localization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedUIElement), true)]
public class LocalizedUIElementEditor : Editor
{
    private SerializedProperty wordIDsProp;
    private SerializedProperty separatorProp;
    private SerializedProperty useMultipleIDsProp;

    private void OnEnable()
    {
        // Получаем SerializedProperty для приватных полей
        wordIDsProp = serializedObject.FindProperty("_wordIDs");
        separatorProp = serializedObject.FindProperty("_separator");
        useMultipleIDsProp = serializedObject.FindProperty("_useMultipleWord");
    }

    public override void OnInspectorGUI()
    {
        var element = (LocalizedUIElement)target;

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(useMultipleIDsProp, new GUIContent("Use Multiple Word IDs"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (element.UseMultipleWord)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(separatorProp, new GUIContent("Separator"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(wordIDsProp, new GUIContent("Word IDs"), true);
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Empty"))
            {
                Undo.RecordObject(element, "Add Word ID");
                element.AddWordID(default);
                EditorUtility.SetDirty(element);
            }
            
            if (GUILayout.Button("Clear All"))
            {
                Undo.RecordObject(element, "Clear Word IDs");
                element.ClearWordIDs();
                EditorUtility.SetDirty(element);
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUI.BeginChangeCheck();
            var wordID = (WordIDs)EditorGUILayout.EnumPopup("Word ID", element.WordID);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(element, "Change Word ID");
                element.WordID = wordID;
                EditorUtility.SetDirty(element);
            }
        }

        DrawPropertiesExcluding(serializedObject, "_wordIDs", "_separator", "_useMultipleWord", "m_Script");
        
        if (serializedObject.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(element);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Update Localization Preview"))
        {
            element.UpdateLocalizationEditor();
        }
    }
}
#endif
