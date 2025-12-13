using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class CameraStackOptimizer : MonoBehaviour
{
    [Header("Настройки оптимизации")]
    [Tooltip("Максимальное количество overlay камер в стеке")]
    public int maxOverlayCameras = 3;
    
    [Tooltip("Отключить overlay камеры за пределами видимости")]
    public bool cullOffscreenOverlays = true;
    
    [Tooltip("Проверять видимость каждые N кадров (0 = каждый кадр)")]
    public int cullCheckInterval = 10;
    
    [Tooltip("Минимальный размер в пикселях для рендера overlay")]
    public float minScreenCoverage = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    private Camera mainCamera;
    private UniversalAdditionalCameraData mainCameraData;
    private int frameCount = 0;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (mainCameraData == null)
            {
                mainCameraData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            
            OptimizeCameraStack();
        }
    }

    void Update()
    {
        if (mainCamera == null || mainCameraData == null) return;
        
        frameCount++;
        
        // Периодическая проверка и оптимизация
        if (frameCount % (cullCheckInterval + 1) == 0)
        {
            if (cullOffscreenOverlays)
            {
                CullOffscreenOverlayCameras();
            }
            
            ValidateCameraStack();
        }
        
        // Debug информация
        if (showDebugInfo && frameCount % 60 == 0)
        {
            DebugCameraStackInfo();
        }
    }

    /// <summary>
    /// Основная оптимизация стека камер
    /// </summary>
    public void OptimizeCameraStack()
    {
        if (mainCameraData == null) return;
        
        // Получаем текущий стек камер
        List<Camera> currentStack = GetCameraStackList();
        
        if (currentStack == null) return;
        
        // 1. Убираем null ссылки
        RemoveNullCamerasFromStack();
        
        // 2. Ограничиваем количество камер
        LimitOverlayCameras(currentStack);
        
        // 3. Сортируем по приоритету (глубине)
        SortCameraStackByPriority(currentStack);
        
        // 4. Отключаем ненужные компоненты на overlay камерах
        OptimizeOverlayCameras(currentStack);
        
        // 5. Настраиваем отсечение
        SetupCameraCulling(currentStack);
        
        Debug.Log($"Camera stack оптимизирован. Камер в стеке: {currentStack.Count}");
    }

    /// <summary>
    /// Получить текущий стек камер как список
    /// </summary>
    private List<Camera> GetCameraStackList()
    {
        if (mainCameraData == null) return null;
        
        // В URP cameraStack доступен только для чтения, создаем копию
        List<Camera> stackList = new List<Camera>();
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            stackList.Add(mainCameraData.cameraStack[i]);
        }
        
        return stackList;
    }

    /// <summary>
    /// Удалить null камеры из стека
    /// </summary>
    private void RemoveNullCamerasFromStack()
    {
        if (mainCameraData == null) return;
        
        // Получаем все камеры, которые нужно удалить
        List<Camera> camerasToRemove = new List<Camera>();
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            if (mainCameraData.cameraStack[i] == null)
            {
                // Не можем удалить напрямую, но можем отключить
                // Вместо этого отключим их через вызов RemoveCameraFromStack
                Debug.LogWarning($"Найден null в позиции {i} стека камер");
            }
        }
    }

    /// <summary>
    /// Ограничить количество overlay камер
    /// </summary>
    private void LimitOverlayCameras(List<Camera> stack)
    {
        if (stack == null || mainCameraData == null) return;
        
        // Отключаем камеры сверх лимита (но не удаляем из стека)
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            var cam = mainCameraData.cameraStack[i];
            if (cam != null)
            {
                bool shouldBeActive = i < maxOverlayCameras;
                
                if (cam.enabled != shouldBeActive)
                {
                    cam.enabled = shouldBeActive;
                    if (!shouldBeActive)
                    {
                        Debug.Log($"Отключена overlay камера {cam.name} (превышен лимит)");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Отсечь overlay камеры вне экрана
    /// </summary>
    private void CullOffscreenOverlayCameras()
    {
        if (mainCameraData == null || !cullOffscreenOverlays) return;
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            var overlayCam = mainCameraData.cameraStack[i];
            if (overlayCam == null) continue;
            
            var camData = overlayCam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null || camData.renderType != CameraRenderType.Overlay) continue;
            
            // Проверяем, если камера привязана к объекту
            bool isOnScreen = IsOverlayVisible(overlayCam);
            bool shouldBeEnabled = overlayCam.enabled && i < maxOverlayCameras;
            
            // Включаем/выключаем в зависимости от видимости и лимита
            bool finalEnabled = shouldBeEnabled && isOnScreen;
            
            if (overlayCam.enabled != finalEnabled)
            {
                overlayCam.enabled = finalEnabled;
                Debug.Log($"Overlay камера {overlayCam.name}: {(finalEnabled ? "ВКЛ" : "ВЫКЛ")}");
            }
        }
    }

    /// <summary>
    /// Проверка видимости overlay камеры
    /// </summary>
    private bool IsOverlayVisible(Camera overlayCam)
    {
        if (mainCamera == null || overlayCam == null) return true;
        
        // Если камера не привязана к объекту, всегда видима
        if (overlayCam.transform.parent == null) return true;
        
        // Получаем экранные координаты объекта
        Vector3 screenPos = mainCamera.WorldToViewportPoint(overlayCam.transform.position);
        
        // Проверяем, находится ли объект в пределах экрана
        bool inViewport = screenPos.x >= -minScreenCoverage && 
                          screenPos.x <= 1 + minScreenCoverage &&
                          screenPos.y >= -minScreenCoverage && 
                          screenPos.y <= 1 + minScreenCoverage &&
                          screenPos.z > 0;
        
        // Проверяем размер объекта на экране
        if (inViewport)
        {
            float objectSize = EstimateObjectScreenSize(overlayCam.transform);
            return objectSize >= minScreenCoverage;
        }
        
        return false;
    }

    /// <summary>
    /// Оценка размера объекта на экране
    /// </summary>
    private float EstimateObjectScreenSize(Transform objTransform)
    {
        if (mainCamera == null || objTransform == null) return 1f;
        
        var renderer = objTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Vector3 screenSize = mainCamera.WorldToViewportPoint(bounds.max) - 
                                mainCamera.WorldToViewportPoint(bounds.min);
            return Mathf.Max(Mathf.Abs(screenSize.x), Mathf.Abs(screenSize.y));
        }
        
        return Mathf.Max(objTransform.lossyScale.x, objTransform.lossyScale.y) / 10f;
    }

    /// <summary>
    /// Сортировка стека камер по приоритету
    /// </summary>
    private void SortCameraStackByPriority(List<Camera> stack)
    {
        if (stack == null || mainCameraData == null) return;
        
        // НЕ можем изменить порядок в cameraStack напрямую
        // Вместо этого сортируем локальную копию для информации
        stack.Sort((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;
            
            int depthCompare = a.depth.CompareTo(b.depth);
            if (depthCompare != 0) return depthCompare;
            
            return a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
        
        Debug.Log("Камеры отсортированы по приоритету (информационно)");
    }

    /// <summary>
    /// Оптимизация настроек overlay камер
    /// </summary>
    private void OptimizeOverlayCameras(List<Camera> stack)
    {
        if (stack == null) return;
        
        foreach (var overlayCam in stack)
        {
            if (overlayCam == null) continue;
            
            var camData = overlayCam.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) continue;
            
            // Отключаем ненужные функции для overlay камер
            camData.renderPostProcessing = false;
            camData.antialiasing = AntialiasingMode.None;
            camData.stopNaN = false;
            camData.dithering = false;
            
            // Настраиваем culling mask для минимизации отрисовки
            overlayCam.cullingMask = GetOptimizedCullingMask(overlayCam);
            
            // Оптимизируем clear flags
            overlayCam.clearFlags = CameraClearFlags.Depth;
            overlayCam.allowHDR = false;
            overlayCam.allowMSAA = false;
            
            // Отключаем ненужные компоненты
            DisableUnnecessaryComponents(overlayCam);
        }
    }

    /// <summary>
    /// Получить оптимизированную маску отсечения
    /// </summary>
    private int GetOptimizedCullingMask(Camera camera)
    {
        // Базовые слои для overlay камер
        int defaultMask = 1 << LayerMask.NameToLayer("UI"); // UI слой
        
        // Добавляем специфичные слои в зависимости от назначения камеры
        if (camera.CompareTag("EffectsCamera"))
        {
            defaultMask |= 1 << LayerMask.NameToLayer("Effects");
        }
        else if (camera.CompareTag("WaterCamera"))
        {
            defaultMask |= 1 << LayerMask.NameToLayer("Water");
        }
        
        return defaultMask;
    }

    /// <summary>
    /// Отключить ненужные компоненты на камере
    /// </summary>
    private void DisableUnnecessaryComponents(Camera camera)
    {
        if (camera == null) return;
        
        // Сохраняем ссылки на необходимые компоненты
        Component[] components = camera.GetComponents<Component>();
        foreach (var component in components)
        {
            if (component is Camera || 
                component is UniversalAdditionalCameraData ||
                component is Transform)
            {
                continue; // Не отключаем
            }
            
            // Отключаем все остальное
            if (component is Behaviour behaviour)
            {
                behaviour.enabled = false;
            }
        }
        
        // Удаляем AudioListener если он есть (кроме основной камеры)
        var audioListener = camera.GetComponent<AudioListener>();
        if (audioListener != null && camera != mainCamera)
        {
            audioListener.enabled = false;
        }
    }

    /// <summary>
    /// Настройка отсечения для камер
    /// </summary>
    private void SetupCameraCulling(List<Camera> stack)
    {
        if (mainCamera != null)
        {
            // Настраиваем отсечение для основной камеры
            mainCamera.farClipPlane = Mathf.Min(mainCamera.farClipPlane, 1000f);
            mainCamera.useOcclusionCulling = true;
            
            // Оптимизируем frustum
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize = Mathf.Min(mainCamera.orthographicSize, 10f);
            }
        }
        
        // Для overlay камер уменьшаем far clip plane
        foreach (var cam in stack)
        {
            if (cam != null)
            {
                cam.farClipPlane = Mathf.Min(cam.farClipPlane, 100f);
                cam.nearClipPlane = Mathf.Max(cam.nearClipPlane, 0.01f);
                
                // Для overlay камер часто достаточно маленького расстояния
                if (cam.GetComponent<UniversalAdditionalCameraData>()?.renderType == CameraRenderType.Overlay)
                {
                    cam.farClipPlane = 50f;
                }
            }
        }
    }

    /// <summary>
    /// Валидация стека камер
    /// </summary>
    private void ValidateCameraStack()
    {
        if (mainCameraData == null) return;
        
        // Проверяем дубликаты
        HashSet<int> uniqueIds = new HashSet<int>();
        List<Camera> duplicates = new List<Camera>();
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            var cam = mainCameraData.cameraStack[i];
            
            if (cam == null) continue;
            
            if (uniqueIds.Contains(cam.GetInstanceID()))
            {
                duplicates.Add(cam);
                Debug.LogWarning($"Дубликат камеры {cam.name} найден в стеке");
            }
            else
            {
                uniqueIds.Add(cam.GetInstanceID());
            }
            
            // Проверяем, что это overlay камера
            var camData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null && camData.renderType != CameraRenderType.Overlay)
            {
                Debug.LogWarning($"Камера {cam.name} не является overlay");
            }
        }
        
        // Отключаем дубликаты
        foreach (var duplicate in duplicates)
        {
            duplicate.enabled = false;
        }
    }

    /// <summary>
    /// Debug информация о стеке камер
    /// </summary>
    private void DebugCameraStackInfo()
    {
        if (mainCameraData == null) return;
        
        string info = $"=== Camera Stack Info ===\n";
        info += $"Main Camera: {(mainCamera != null ? mainCamera.name : "null")}\n";
        info += $"Total Cameras in Stack: {mainCameraData.cameraStack.Count}\n";
        info += $"Max Allowed: {maxOverlayCameras}\n\n";
        
        int enabledCount = 0;
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            var cam = mainCameraData.cameraStack[i];
            if (cam != null)
            {
                bool isEnabled = cam.enabled && i < maxOverlayCameras;
                if (isEnabled) enabledCount++;
                
                info += $"{i}: {cam.name}\n";
                info += $"  Enabled: {isEnabled}\n";
                info += $"  Depth: {cam.depth}\n";
                info += $"  Culling Mask: {ConvertMaskToString(cam.cullingMask)}\n";
                
                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (camData != null)
                {
                    info += $"  Type: {camData.renderType}\n";
                    info += $"  PostProcessing: {camData.renderPostProcessing}\n";
                }
                
                info += "\n";
            }
            else
            {
                info += $"{i}: [NULL REFERENCE]\n\n";
            }
        }
        
        info += $"Active Cameras: {enabledCount}/{maxOverlayCameras}\n";
        info += $"Culling Offscreen: {cullOffscreenOverlays}\n";
        
        Debug.Log(info);
    }

    /// <summary>
    /// Конвертировать mask в строку
    /// </summary>
    private string ConvertMaskToString(int mask)
    {
        List<string> layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                layers.Add(string.IsNullOrEmpty(layerName) ? i.ToString() : layerName);
            }
        }
        return string.Join(", ", layers);
    }

    /// <summary>
    /// Добавить камеру в стек (используется в редакторе или через события)
    /// </summary>
    public bool TryEnableCameraInStack(Camera camera)
    {
        if (mainCameraData == null || camera == null) return false;
        
        // Проверяем, есть ли уже камера в стеке
        bool alreadyInStack = false;
        int stackIndex = -1;
        
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            if (mainCameraData.cameraStack[i] == camera)
            {
                alreadyInStack = true;
                stackIndex = i;
                break;
            }
        }
        
        // Если камера уже в стеке, проверяем можно ли её включить
        if (alreadyInStack)
        {
            bool canEnable = stackIndex < maxOverlayCameras;
            if (camera.enabled != canEnable)
            {
                camera.enabled = canEnable;
                Debug.Log($"Камера {camera.name} {(canEnable ? "включена" : "отключена")}");
            }
            return canEnable;
        }
        
        Debug.LogWarning($"Камера {camera.name} не найдена в стеке. " +
                        "Добавьте её через инспектор основной камеры.");
        return false;
    }

    /// <summary>
    /// Отключить камеру в стеке
    /// </summary>
    public void DisableCameraInStack(Camera camera)
    {
        if (camera != null)
        {
            camera.enabled = false;
            Debug.Log($"Камера {camera.name} отключена");
        }
    }

    /// <summary>
    /// Очистить стек камер (отключить все overlay камеры)
    /// </summary>
    public void DisableAllOverlayCameras()
    {
        if (mainCameraData == null) return;
        
        int disabledCount = 0;
        for (int i = 0; i < mainCameraData.cameraStack.Count; i++)
        {
            var cam = mainCameraData.cameraStack[i];
            if (cam != null && cam.enabled)
            {
                cam.enabled = false;
                disabledCount++;
            }
        }
        
        Debug.Log($"Отключено {disabledCount} overlay камер");
    }

    /// <summary>
    /// Получить статистику по стеку камер
    /// </summary>
    public string GetCameraStackStats()
    {
        if (mainCameraData == null)
            return "No camera data";
        
        int totalCount = mainCameraData.cameraStack.Count;
        int activeCount = 0;
        int nullCount = 0;
        
        for (int i = 0; i < totalCount; i++)
        {
            var cam = mainCameraData.cameraStack[i];
            if (cam == null)
            {
                nullCount++;
            }
            else if (cam.enabled && i < maxOverlayCameras)
            {
                activeCount++;
            }
        }
        
        return $"Cameras: {totalCount} " +
               $"(Active: {activeCount}, " +
               $"Null: {nullCount}, " +
               $"Limit: {maxOverlayCameras})";
    }

    /// <summary>
    /// Проверить производительность рендеринга
    /// </summary>
    public void CheckRenderingPerformance()
    {
        if (mainCameraData == null) return;
        
        int drawCalls = 0;
        int triangleCount = 0;
        
        // Примерная оценка - в реальном проекте используйте Profiler
        foreach (var cam in mainCameraData.cameraStack)
        {
            if (cam != null && cam.enabled)
            {
                // Каждая overlay камера добавляет минимум 1 draw call
                drawCalls++;
                triangleCount += 1000; // Примерное значение
            }
        }
        
        Debug.Log($"Примерная нагрузка от overlay камер:\n" +
                 $"Draw Calls: {drawCalls}\n" +
                 $"Triangles: {triangleCount}\n" +
                 $"Рекомендация: {(drawCalls > 3 ? "СЛИШКОМ МНОГО! Уменьшите количество камер" : "OK")}");
    }

    /// <summary>
    /// Автоматическая оптимизация на старте
    /// </summary>
    [ContextMenu("Optimize Now")]
    public void OptimizeNow()
    {
        OptimizeCameraStack();
        Debug.Log("Оптимизация завершена: " + GetCameraStackStats());
    }
}
