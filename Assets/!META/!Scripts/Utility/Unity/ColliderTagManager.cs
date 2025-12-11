using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Physics/Collider Tag Synchronizer")]
public class ColliderTagSynchronizer : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Tag that triggers collider addition")]
    private string _colliderTag = "Collider";

    [SerializeField]
    [Tooltip("Automatically update in Editor mode")]
    private bool _editorAutoUpdate = true;

    [SerializeField]
    [Tooltip("Update during runtime")]
    private bool _runtimeUpdates = true;

    [SerializeField]
    [Tooltip("Draw collider gizmos in editor")]
    private bool _drawGizmos = true;

    [SerializeField]
    [Tooltip("Color for collider gizmos")]
    private Color _gizmoColor = new Color(0, 1, 0, 0.3f);

    [SerializeField]
    [Tooltip("Gizmo wireframe color")]
    private Color _gizmoWireColor = new Color(0, 1, 0, 1f);

    [SerializeField]
    [Tooltip("Show only when selected")]
    private bool _onlyDrawWhenSelected = true;

    private List<Transform> _taggedChildren = null;
    private List<Collider> _allTaggedColliders = null;
    private Transform _cachedTransform = null;
    private bool _isDirty = true;

    public IReadOnlyList<Transform> TaggedChildren
    {
        get
        {
            EnsureInitialized();
            return _taggedChildren ?? (_taggedChildren = new List<Transform>());
        }
    }

    public IReadOnlyList<Collider> AllTaggedColliders
    {
        get
        {
            EnsureInitialized();
            return _allTaggedColliders ?? (_allTaggedColliders = new List<Collider>());
        }
    }

    private void EnsureInitialized()
    {
        if (_cachedTransform == null)
        {
            _cachedTransform = transform;
        }

        if (_taggedChildren == null)
        {
            _taggedChildren = new List<Transform>();
        }

        if (_allTaggedColliders == null)
        {
            _allTaggedColliders = new List<Collider>();
        }
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();

        if (ShouldUpdate())
        {
            ForceUpdate();
        }
    }

    private void OnTransformChildrenChanged()
    {
        EnsureInitialized();

        if (ShouldUpdate())
        {
            _isDirty = true;
            if (Application.isPlaying)
            {
                UpdateColliderStates();
            }
#if UNITY_EDITOR
            else if (_editorAutoUpdate)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (this != null) UpdateColliderStates();
                };
            }
#endif
        }
    }

    private bool ShouldUpdate()
    {
        if (Application.isPlaying)
            return _runtimeUpdates;

        return _editorAutoUpdate;
    }

    [ContextMenu("Force Update Colliders")]
    public void ForceUpdate()
    {
        _isDirty = true;
        UpdateColliderStates();
    }

    public void UpdateColliderStates()
    {
        EnsureInitialized();

        if (!_isDirty && Application.isPlaying)
            return;

        if (string.IsNullOrWhiteSpace(_colliderTag))
        {
            Debug.LogWarning($"[{nameof(ColliderTagSynchronizer)}] No collider tag specified on {name}", this);
            return;
        }

        CacheTaggedChildren();
        UpdateColliders();
        _isDirty = false;
    }

    // Кэширование детей с нужным тегом для оптимизации
    private void CacheTaggedChildren()
    {
        if (_cachedTransform == null)
        {
            _cachedTransform = transform;
        }

        if (_taggedChildren == null)
        {
            _taggedChildren = new List<Transform>();
        }

        if (_allTaggedColliders == null)
        {
            _allTaggedColliders = new List<Collider>();
        }

        _taggedChildren.Clear();
        _allTaggedColliders.Clear();

        int childCount = _cachedTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = _cachedTransform.GetChild(i);
            if (child != null && child.CompareTag(_colliderTag))
            {
                _taggedChildren.Add(child);

                // Получаем ВСЕ BoxCollider на объекте
                var colliders = child.GetComponents<BoxCollider>();
                var meshCollider = child.GetComponent<MeshCollider>();
                foreach (var collider in colliders)
                {
                    if (collider != null)
                    {
                        _allTaggedColliders.Add(collider);
                    }

                    if (meshCollider != null)
                    {
                        _allTaggedColliders.Add(meshCollider);
                    }
                }
            }
        }
    }

    private void UpdateColliders()
    {
        // Убедимся, что у каждого объекта с тегом есть хотя бы один BoxCollider
        foreach (Transform child in _taggedChildren)
        {
            if (child == null) continue;

            // Получаем все BoxCollider на объекте
            var colliders = child.GetComponents<BoxCollider>();

            // Если нет ни одного коллайдера, добавляем
            if (colliders.Length == 0)
            {
                AttachColliderComponent(child);
            }
        }

        // Удаляем все коллайдеры у детей без нужного тега
        if (_cachedTransform != null)
        {
            for (int i = 0; i < _cachedTransform.childCount; i++)
            {
                Transform child = _cachedTransform.GetChild(i);
                if (child == null) continue;

                if (!child.CompareTag(_colliderTag))
                {
                    // Удаляем все BoxCollider на объекте
                    var colliders = child.GetComponents<BoxCollider>();
                    foreach (var collider in colliders)
                    {
                        if (collider != null)
                        {
                            DetachColliderComponent(collider);
                        }
                    }
                }
            }
        }

        // Обновляем кэш коллайдеров
        CacheTaggedChildren();
    }

    private void AttachColliderComponent(Transform child)
    {
        if (child == null)
            return;

        // Оптимизация: проверяем, не добавлен ли уже коллайдер
        if (child.GetComponent<BoxCollider>() != null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.AddComponent<BoxCollider>(child.gameObject);
        }
        else
        {
            child.gameObject.AddComponent<BoxCollider>();
        }
#else
        child.gameObject.AddComponent<BoxCollider>();
#endif
    }

    private void DetachColliderComponent(BoxCollider collider)
    {
        if (collider == null)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.Undo.DestroyObjectImmediate(collider);
        }
        else
        {
            Destroy(collider);
        }
#else
        Destroy(collider);
#endif
    }

    [ContextMenu("Remove All Child Colliders")]
    public void RemoveAllChildColliders()
    {
        EnsureInitialized();

        if (_cachedTransform == null)
            return;

        for (int i = 0; i < _cachedTransform.childCount; i++)
        {
            Transform child = _cachedTransform.GetChild(i);
            if (child == null) continue;

            // Получаем все BoxCollider на объекте
            var colliders = child.GetComponents<BoxCollider>();

            foreach (var collider in colliders)
            {
                if (collider != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        UnityEditor.Undo.DestroyObjectImmediate(collider);
                    }
                    else
                    {
                        Destroy(collider);
                    }
#else
                    Destroy(collider);
#endif
                }
            }
        }

        _isDirty = true;
        CacheTaggedChildren();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_editorAutoUpdate && !Application.isPlaying)
        {
            _isDirty = true;
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    EnsureInitialized();
                    UpdateColliderStates();
                }
            };
        }
    }

    // Отрисовка Gizmos для визуализации коллайдеров
    private void OnDrawGizmosSelected()
    {
        if (!_drawGizmos || _onlyDrawWhenSelected == false)
            return;

        EnsureInitialized();

        if (_allTaggedColliders == null || _allTaggedColliders.Count == 0)
            return;

        DrawAllColliders();
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || _onlyDrawWhenSelected)
            return;

        EnsureInitialized();

        if (_allTaggedColliders == null || _allTaggedColliders.Count == 0)
            return;

        // Для невыделенных объектов используем более прозрачный цвет
        Color originalSolidColor = _gizmoColor;
        Color originalWireColor = _gizmoWireColor;

        _gizmoColor = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, _gizmoColor.a * 0.2f);
        _gizmoWireColor = new Color(_gizmoWireColor.r, _gizmoWireColor.g, _gizmoWireColor.b, _gizmoWireColor.a * 0.5f);

        DrawAllColliders();

        // Восстанавливаем цвета
        _gizmoColor = originalSolidColor;
        _gizmoWireColor = originalWireColor;
    }

    private void DrawAllColliders()
    {
        foreach (var collider in _allTaggedColliders)
        {
            if (collider == null || !collider.enabled)
                continue;

            if (collider is BoxCollider boxCollider)
                DrawColliderGizmo(boxCollider);
        }
    }

    private void DrawColliderGizmo(BoxCollider collider)
    {
        if (collider == null || collider.transform == null)
            return;

        var originalMatrix = Gizmos.matrix;
        var colliderTransform = collider.transform;

        var fixSize = 0.001f;
        var fixSizeVector = new Vector3(fixSize, fixSize, fixSize);
        var sizeCollider = fixSizeVector + colliderTransform.lossyScale;
        var colliderMatrix = Matrix4x4.TRS(
            colliderTransform.TransformPoint(collider.center),
            colliderTransform.rotation,
            Vector3.Scale(sizeCollider, collider.size + fixSizeVector)
        );

        Gizmos.matrix = colliderMatrix;

        Gizmos.color = _gizmoColor;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);

        Gizmos.color = _gizmoWireColor;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = originalMatrix;
    }
#endif
}
