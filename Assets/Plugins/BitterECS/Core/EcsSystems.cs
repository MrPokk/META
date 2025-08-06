using System;
using System.Collections.Generic;
using BitterECS.Utility;

namespace BitterECS.Core
{
    public sealed class EcsSystems : IInitialize, IDisposable
    {
        private static readonly List<IEcsSystem> s_systems = new(EcsConfig.InitialSystemsCapacity);
        private static readonly Dictionary<Type, IEcsSystem[]> s_cachedInstanceSystems = new(EcsConfig.InitialSystemsCapacity);

        public void Init()
        {
            LoadAllSystems();
        }

        public static void Run<T>(Action<T> action) where T : class, IEcsSystem
        {
            var systems = GetSystems<T>();
            foreach (var system in systems)
            {
                action(system);
            }
        }

        public static IReadOnlyCollection<T> GetSystems<T>() where T : class, IEcsSystem
        {
            var type = typeof(T);

            if (s_cachedInstanceSystems.TryGetValue(type, out var cached))
            {
                return (T[])cached;
            }

            var result = new List<T>(s_systems.Count);

            foreach (var system in s_systems)
            {
                if (system is T typedSystem)
                {
                    result.Add(typedSystem);
                }
            }

            var cachedResult = result.ToArray();
            s_cachedInstanceSystems[type] = cachedResult;

            return cachedResult;
        }


        private void LoadAllSystems()
        {
            s_systems.Clear();

            var systemTypes = ReflectionUtility.FindAllAssignments<IEcsSystem>();
            foreach (var type in systemTypes)
            {
#if UNITY_2020_1_OR_NEWER
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    continue;
                }
#endif
                if (Activator.CreateInstance(type) is IEcsSystem system)
                {
                    s_systems.Add(system);
                }
            }

            s_systems.Sort((left, right) => (int)left.PrioritySystem - (int)right.PrioritySystem);
            s_cachedInstanceSystems.Clear();
        }


        public void Dispose()
        {
            foreach (var system in s_systems)
            {
                if (system is IDisposable disposableSystem)
                {
                    disposableSystem.Dispose();
                }
            }
            s_systems.Clear();
            s_cachedInstanceSystems.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
