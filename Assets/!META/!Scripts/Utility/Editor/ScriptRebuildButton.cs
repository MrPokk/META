#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class ScriptRebuildButton : EditorWindow
{
    [SerializeField] private bool _clearConsole = true;
    [SerializeField] private bool _disableDomainReload = false;
    [SerializeField] private bool _autoRefreshAssets = true;
    [SerializeField] private bool _logProgress = true;

    [MenuItem("Tools/Rebuild Scripts")]
    public static void ShowWindow()
    {
        GetWindow<ScriptRebuildButton>("Script Rebuild").Show();
    }

    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Rebuild Scripts", GUILayout.Height(30)))
        {
            RebuildScripts();
        }

        _disableDomainReload = EditorGUILayout.Toggle("Disable Domain Reload", _disableDomainReload);
        _autoRefreshAssets = EditorGUILayout.Toggle("Auto Refresh Assets", _autoRefreshAssets);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        _clearConsole = EditorGUILayout.Toggle("Clear Console Before Rebuild", _clearConsole);
        _logProgress = EditorGUILayout.Toggle("Log Progress", _logProgress);

        if (EditorGUI.EndChangeCheck())
        {
            ApplySettings();
        }

        GUILayout.Space(20);
    }

    private void ApplySettings()
    {
        EditorSettings.enterPlayModeOptionsEnabled = _disableDomainReload;
        if (_disableDomainReload)
        {
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        }

        if (_autoRefreshAssets)
        {
            AssetDatabase.AllowAutoRefresh();
        }
        else
        {
            AssetDatabase.DisallowAutoRefresh();
        }

        if (_logProgress)
        {
            Debug.Log($"Settings applied: AutoRefresh={_autoRefreshAssets}, DomainReload={!_disableDomainReload}");
        }
    }

    private void RebuildScripts()
    {
        if (_clearConsole)
        {
            ClearConsole();
        }

        if (_logProgress)
        {
            Debug.Log("Starting script rebuild process...");
        }

        try
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (_logProgress)
            {
                Debug.Log("Scripts rebuilt successfully!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Script rebuild failed: {e.Message}");
        }
    }

    private static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}
#endif
