using System;
using System.Collections.Generic;
using BitterECS.Utility;
using UnityEngine;

namespace BitterECS.Core.Integration
{
    public class EcsUnityViewDatabase
    {
        private static bool s_isInitialized;
        private readonly static Dictionary<Type, ILinkableView> s_viewPrefabs = new();

        private static void EnsureInitialized()
        {
            if (s_isInitialized)
                return;

            Initialize();
        }

        public static void Initialize(bool forceUpdate = false)
        {
            if (s_isInitialized && !forceUpdate)
                return;

            try
            {
                if (forceUpdate)
                    s_viewPrefabs.Clear();

                var allGameObjects = Resources.LoadAll<GameObject>(PathProject.VIEWS);
                if (allGameObjects == null || allGameObjects.Length == 0)
                {
                    Debug.LogWarning("No ECS views found at path:" + PathProject.VIEWS);
                    return;
                }

                foreach (var viewPrefab in allGameObjects)
                {
                    if (!viewPrefab)
                        continue;

                    var ecsView = viewPrefab.GetComponent<ILinkableView>();
                    if (ecsView == null)
                    {
                        Debug.LogError($"EcsView component missing in prefab: {viewPrefab.name}");
                        continue;
                    }

                    s_viewPrefabs.TryAdd(ecsView.GetType(), ecsView);
                }

                s_isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"ECS View Database initialization failed: {ex.Message}", ex);
            }
        }
        
        public static IEnumerable<(Type type, MonoBehaviour monoBehaviour, ILinkableView linkableView)> GetAll()
        {
            EnsureInitialized();

            foreach (var kvp in s_viewPrefabs)
            {
                yield return (kvp.Key, kvp.Value as MonoBehaviour, kvp.Value);
            }
        }

        public static (MonoBehaviour monoBehaviour, ILinkableView linkableView) GetPrefab(Type viewType)
        {
            EnsureInitialized();

            if (!s_viewPrefabs.TryGetValue(viewType, out var prefab))
            {
                Debug.LogError($"ECS View of type {viewType.Name} not found in database");
                return (null, null);
            }

            if (prefab == null)
            {
                Debug.LogError($"ECS View prefab is null. Check path: Resources/EcsViews");
                return (null, null);
            }

            return (prefab as MonoBehaviour, prefab);
        }

        public static (T monoBehaviour, ILinkableView linkableView) GetPrefab<T>() where T : MonoBehaviour
        {
            var result = GetPrefab(typeof(T));
            return (result.monoBehaviour as T, result.linkableView);
        }

        public static (MonoBehaviour monoBehaviour, ILinkableView linkableView) GetInstance(Type viewType, Vector3 position = default, Quaternion rotation = default)
        {
            var prefab = GetPrefab(viewType);
            if (prefab.monoBehaviour == null)
                return (null, null);

            var newInstance = UnityEngine.Object.Instantiate(prefab.monoBehaviour, position, rotation);
            var linkableView = newInstance.GetComponent<ILinkableView>();
            return (newInstance, linkableView);
        }

        public static (T monoBehaviour, ILinkableView linkableView) GetInstance<T>(Vector3 position = default, Quaternion rotation = default) where T : MonoBehaviour
        {
            var result = GetInstance(typeof(T), position, rotation);
            return (result.monoBehaviour as T, result.linkableView);
        }
    }
}
