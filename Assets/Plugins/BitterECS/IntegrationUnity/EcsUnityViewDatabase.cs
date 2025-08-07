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

        public static ILinkableView GetPrefab(Type viewType)
        {
            EnsureInitialized();

            if (!s_viewPrefabs.TryGetValue(viewType, out var prefab))
                throw new KeyNotFoundException($"ECS View of type {viewType.Name} not found in database");

            if (prefab == null)
                throw new ArgumentNullException($"ECS View prefab is null. Check path: Resources/EcsViews");

            return prefab;
        }

        public static T GetPrefab<T>() where T : ILinkableView => (T)GetPrefab(typeof(T));

        public static ILinkableView GetInstance(Type viewType)
        {
            var prefab = GetPrefab(viewType);
            var prefabUnity = prefab as MonoBehaviour;
            var newInstance = UnityEngine.Object.Instantiate(prefabUnity);
            return newInstance as ILinkableView;
        }

        public static T GetInstance<T>() where T : ILinkableView => (T)GetInstance(typeof(T));
    }
}
