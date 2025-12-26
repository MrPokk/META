using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // Работает и в редакторе, и в игре
public class AutoMaterialAssigner : MonoBehaviour
{
    [Header("Настройки материала")]
    [Tooltip("Имя материала без суффикса (например, 'белый')")]
    public string baseMaterialName = "белый";
    
    [Tooltip("Папка с материалами (относительно Assets)")]
    public string materialsFolder = "Assets/!META/Art/Models/Floors/5Floor/Materials";
    
    [Header("Настройки объекта")]
    [Tooltip("Автоматически искать материал при старте")]
    public bool findMaterialOnStart = true;
    
    [Tooltip("Автоматически применять материал при старте")]
    public bool applyMaterialOnStart = true;
    
    [Header("Отладка")]
    [Tooltip("Показывать сообщения в консоли")]
    public bool debugLog = true;
    
    private Material targetMaterial;
    private Renderer objectRenderer;
    
    void Start()
    {
        if (Application.isPlaying && findMaterialOnStart)
        {
            FindAndAssignMaterial();
        }
    }
    
    void OnEnable()
    {
        // В редакторе тоже пытаемся найти материал
        if (!Application.isPlaying)
        {
            FindAndAssignMaterial();
        }
    }
    
    public void FindAndAssignMaterial()
    {
        if (string.IsNullOrEmpty(baseMaterialName))
        {
            if (debugLog) Debug.LogWarning($"{gameObject.name}: Не указано имя материала");
            return;
        }
        
        // Получаем рендерер
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            if (debugLog) Debug.LogWarning($"{gameObject.name}: Нет компонента Renderer");
            return;
        }
        
        // Ищем материал
        FindMaterial();
        
        // Применяем материал
        if (applyMaterialOnStart && targetMaterial != null)
        {
            ApplyMaterial();
        }
    }
    
    private void FindMaterial()
    {
        // Сначала ищем материал без суффикса
        string materialPath = $"{materialsFolder}/{baseMaterialName}.mat";
        targetMaterial = Resources.Load<Material>(GetResourcesPath(materialPath));
        
        if (targetMaterial == null)
        {
            // Пробуем .001
            materialPath = $"{materialsFolder}/{baseMaterialName}.001.mat";
            targetMaterial = Resources.Load<Material>(GetResourcesPath(materialPath));
        }
        
        // Если не нашли через Resources, пробуем через AssetDatabase (только в редакторе)
        #if UNITY_EDITOR
        if (targetMaterial == null && !Application.isPlaying)
        {
            materialPath = $"{materialsFolder}/{baseMaterialName}.mat";
            targetMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            
            if (targetMaterial == null)
            {
                materialPath = $"{materialsFolder}/{baseMaterialName}.001.mat";
                targetMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
        }
        #endif
        
        if (targetMaterial == null && debugLog)
        {
            Debug.LogWarning($"{gameObject.name}: Не найден материал '{baseMaterialName}' в папке {materialsFolder}");
        }
        else if (debugLog)
        {
            Debug.Log($"{gameObject.name}: Найден материал {targetMaterial.name}");
        }
    }
    
    private string GetResourcesPath(string fullPath)
    {
        // Конвертируем путь в путь для Resources.Load
        string resourcesPath = fullPath;
        
        // Убираем "Assets/"
        if (resourcesPath.StartsWith("Assets/"))
        {
            resourcesPath = resourcesPath.Substring(7);
        }
        
        // Убираем расширение
        if (resourcesPath.EndsWith(".mat"))
        {
            resourcesPath = resourcesPath.Substring(0, resourcesPath.Length - 4);
        }
        
        // Убираем "Resources/" если есть
        if (resourcesPath.Contains("Resources/"))
        {
            resourcesPath = resourcesPath.Substring(resourcesPath.IndexOf("Resources/") + 10);
        }
        
        return resourcesPath;
    }
    
    public void ApplyMaterial()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponent<Renderer>();
        }
        
        if (objectRenderer != null && targetMaterial != null)
        {
            objectRenderer.sharedMaterial = targetMaterial;
            
            if (debugLog)
            {
                Debug.Log($"{gameObject.name}: Материал {targetMaterial.name} применен");
            }
        }
        else
        {
            if (debugLog) Debug.LogWarning($"{gameObject.name}: Не могу применить материал");
        }
    }
    
    public void FindAndApplyMaterial()
    {
        FindMaterial();
        ApplyMaterial();
    }
    
    #if UNITY_EDITOR
    // Кнопка в инспекторе для ручного применения
    [UnityEditor.CustomEditor(typeof(AutoMaterialAssigner))]
    public class AutoMaterialAssignerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            AutoMaterialAssigner script = (AutoMaterialAssigner)target;
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Найти и применить материал"))
            {
                script.FindAndApplyMaterial();
            }
            
            if (GUILayout.Button("Только найти материал"))
            {
                script.FindMaterial();
            }
            
            if (GUILayout.Button("Применить материал"))
            {
                script.ApplyMaterial();
            }
        }
    }
    #endif
}
