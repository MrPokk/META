using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MaterialCleanupWithReplacement : EditorWindow
{
    private string targetFolder = "Assets/!META/Art/Models/Floors/5Floor/Materials";
    
    // Статистика
    private int materialsDeleted = 0;
    private int referencesUpdated = 0;
    private List<string> logMessages = new List<string>();

    [MenuItem("Tools/Cleanup Materials with Replacement")]
    public static void ShowWindow()
    {
        GetWindow<MaterialCleanupWithReplacement>("Material Cleanup with Replacement");
    }

    void OnGUI()
    {
        GUILayout.Label("Material Cleanup with Automatic Replacement", EditorStyles.boldLabel);
        
        targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Scan and Cleanup with Replacement", GUILayout.Height(40)))
        {
            ScanAndCleanupWithReplacement();
        }
        
        if (GUILayout.Button("Dry Run (Show What Will Be Done)"))
        {
            DryRun();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Log:", EditorStyles.boldLabel);
        
        // Показать лог
        Vector2 scrollPos = EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(200));
        foreach (string message in logMessages)
        {
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
        }
        EditorGUILayout.EndScrollView();
        
        if (GUILayout.Button("Clear Log"))
        {
            logMessages.Clear();
        }
    }

    private void ScanAndCleanupWithReplacement()
    {
        materialsDeleted = 0;
        referencesUpdated = 0;
        logMessages.Clear();
        
        AddLog("Начинаем очистку материалов с заменой ссылок...");
        
        if (!Directory.Exists(targetFolder))
        {
            AddLog($"Ошибка: Папка не существует: {targetFolder}", true);
            return;
        }

        // Получаем все материалы в папке
        string[] allMaterialPaths = Directory.GetFiles(targetFolder, "*.mat", SearchOption.TopDirectoryOnly);
        
        if (allMaterialPaths.Length == 0)
        {
            AddLog("Материалы не найдены в указанной папке");
            return;
        }

        AddLog($"Найдено материалов: {allMaterialPaths.Length}");
        
        // Создаем словарь для замены материалов
        Dictionary<string, Material> replacementMap = CreateReplacementMap(allMaterialPaths);
        
        if (replacementMap.Count == 0)
        {
            AddLog("Нет материалов для замены");
            return;
        }
        
        AddLog($"Создана карта замены для {replacementMap.Count} материалов");
        
        // Обновляем ссылки во всех объектах сцены
        UpdateReferencesInScene(replacementMap);
        
        // Обновляем ссылки в префабах
        UpdateReferencesInPrefabs(replacementMap);
        
        // Удаляем старые материалы
        DeleteOldMaterials(replacementMap);
        
        // Применяем изменения
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        AddLog($"Готово! Удалено материалов: {materialsDeleted}, обновлено ссылок: {referencesUpdated}");
    }

    private Dictionary<string, Material> CreateReplacementMap(string[] materialPaths)
    {
        Dictionary<string, Material> replacementMap = new Dictionary<string, Material>();
        
        // Группируем материалы по базовому имени
        Dictionary<string, List<MaterialInfo>> materialGroups = new Dictionary<string, List<MaterialInfo>>();

        foreach (string path in materialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) continue;
            
            string fileName = Path.GetFileNameWithoutExtension(path);
            MaterialInfo info = new MaterialInfo
            {
                Path = path,
                Name = fileName,
                Material = material
            };
            
            string baseName = GetBaseName(fileName);
            
            if (!materialGroups.ContainsKey(baseName))
            {
                materialGroups[baseName] = new List<MaterialInfo>();
            }
            
            materialGroups[baseName].Add(info);
        }

        // Для каждой группы определяем, какие материалы заменять
        foreach (var group in materialGroups)
        {
            string baseName = group.Key;
            List<MaterialInfo> materials = group.Value;
            
            if (materials.Count <= 1) continue;
            
            // Сортируем по имени
            materials.Sort((a, b) => a.Name.CompareTo(b.Name));
            
            // Определяем материал, который останется
            MaterialInfo keepMaterial = null;
            
            // Ищем материал без суффикса
            keepMaterial = materials.Find(m => m.Name == baseName);
            
            // Если нет без суффикса, берем .001
            if (keepMaterial == null)
            {
                keepMaterial = materials.Find(m => m.Name == baseName + ".001");
            }
            
            // Если нашли материал для сохранения, добавляем остальные в карту замены
            if (keepMaterial != null)
            {
                foreach (MaterialInfo materialInfo in materials)
                {
                    if (materialInfo != keepMaterial)
                    {
                        replacementMap[materialInfo.Path] = keepMaterial.Material;
                        AddLog($"Будет заменен: {materialInfo.Name} -> {keepMaterial.Name}");
                    }
                }
            }
        }
        
        return replacementMap;
    }

    private void UpdateReferencesInScene(Dictionary<string, Material> replacementMap)
    {
        AddLog("Обновление ссылок в активной сцене...");
        
        // Получаем все рендереры в сцене
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        
        foreach (Renderer renderer in allRenderers)
        {
            UpdateRendererMaterials(renderer, replacementMap);
        }
        
        AddLog($"Проверено рендереров в сцене: {allRenderers.Length}");
    }

    private void UpdateReferencesInPrefabs(Dictionary<string, Material> replacementMap)
    {
        AddLog("Обновление ссылок в префабах...");
        
        // Находим все префабы в проекте
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int prefabCount = 0;
        
        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null) continue;
            
            // Получаем все рендереры в префабе
            Renderer[] prefabRenderers = prefab.GetComponentsInChildren<Renderer>(true);
            
            bool prefabModified = false;
            
            foreach (Renderer renderer in prefabRenderers)
            {
                if (UpdateRendererMaterials(renderer, replacementMap))
                {
                    prefabModified = true;
                }
            }
            
            // Сохраняем изменения в префабе
            if (prefabModified)
            {
                PrefabUtility.SavePrefabAsset(prefab);
                prefabCount++;
            }
        }
        
        AddLog($"Обновлено префабов: {prefabCount}");
    }

    private bool UpdateRendererMaterials(Renderer renderer, Dictionary<string, Material> replacementMap)
    {
        bool modified = false;
        
        // Проверяем общий материал
        if (renderer.sharedMaterial != null)
        {
            string materialPath = AssetDatabase.GetAssetPath(renderer.sharedMaterial);
            
            if (replacementMap.ContainsKey(materialPath))
            {
                renderer.sharedMaterial = replacementMap[materialPath];
                referencesUpdated++;
                modified = true;
            }
        }
        
        // Проверяем массив материалов
        if (renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
        {
            Material[] materials = renderer.sharedMaterials;
            bool arrayModified = false;
            
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    string materialPath = AssetDatabase.GetAssetPath(materials[i]);
                    
                    if (replacementMap.ContainsKey(materialPath))
                    {
                        materials[i] = replacementMap[materialPath];
                        referencesUpdated++;
                        arrayModified = true;
                    }
                }
            }
            
            if (arrayModified)
            {
                renderer.sharedMaterials = materials;
                modified = true;
            }
        }
        
        return modified;
    }

    private void DeleteOldMaterials(Dictionary<string, Material> replacementMap)
    {
        AddLog("Удаление старых материалов...");
        
        foreach (string materialPath in replacementMap.Keys)
        {
            if (File.Exists(materialPath))
            {
                AssetDatabase.DeleteAsset(materialPath);
                materialsDeleted++;
                AddLog($"Удален: {Path.GetFileName(materialPath)}");
            }
        }
    }

    private void DryRun()
    {
        materialsDeleted = 0;
        referencesUpdated = 0;
        logMessages.Clear();
        
        AddLog("=== DRY RUN (без реальных изменений) ===");
        
        if (!Directory.Exists(targetFolder))
        {
            AddLog($"Ошибка: Папка не существует: {targetFolder}", true);
            return;
        }

        string[] allMaterialPaths = Directory.GetFiles(targetFolder, "*.mat", SearchOption.TopDirectoryOnly);
        
        if (allMaterialPaths.Length == 0)
        {
            AddLog("Материалы не найдены");
            return;
        }
        
        AddLog($"Найдено материалов: {allMaterialPaths.Length}");
        
        // Анализ без реальных изменений
        Dictionary<string, List<string>> materialGroups = new Dictionary<string, List<string>>();

        foreach (string path in allMaterialPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string baseName = GetBaseName(fileName);
            
            if (!materialGroups.ContainsKey(baseName))
            {
                materialGroups[baseName] = new List<string>();
            }
            
            materialGroups[baseName].Add(fileName);
        }

        foreach (var group in materialGroups)
        {
            if (group.Value.Count > 1)
            {
                AddLog($"Группа '{group.Key}': {string.Join(", ", group.Value)}");
                
                // Определяем что оставить
                string keepMaterial = group.Value.Contains(group.Key) ? group.Key : group.Key + ".001";
                
                foreach (string materialName in group.Value)
                {
                    if (materialName != keepMaterial)
                    {
                        AddLog($"  Будет удален: {materialName}");
                        materialsDeleted++;
                    }
                }
            }
        }
        
        AddLog($"=== ИТОГО DRY RUN ===");
        AddLog($"Будет удалено материалов: {materialsDeleted}");
        AddLog($"Для реального выполнения нажмите 'Scan and Cleanup with Replacement'");
    }

    private string GetBaseName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return fileName;

        for (int i = fileName.Length - 1; i >= 0; i--)
        {
            if (fileName[i] == '.')
            {
                string suffix = fileName.Substring(i + 1);
                if (int.TryParse(suffix, out _))
                {
                    return fileName.Substring(0, i);
                }
                break;
            }
        }
        
        return fileName;
    }

    private void AddLog(string message, bool isError = false)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";
        
        logMessages.Add(logMessage);
        
        if (isError)
            Debug.LogError(message);
        else
            Debug.Log(message);
    }

    private class MaterialInfo
    {
        public string Path;
        public string Name;
        public Material Material;
    }
}
