using System.Runtime.CompilerServices;

namespace BitterECS.Core
{
    public static class EcsPoolExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddOrReplace<T>(this EcsPool<T> pool, int entityId, in T component) where T : struct
        {
            if (pool.Has(entityId))
            {
                pool.Get(entityId) = component;
            }
            else
            {
                pool.Add(entityId, component);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetOrAddDefault<T>(this EcsPool<T> pool, int entityId) where T : struct
        {
            if (!pool.Has(entityId))
            {
                pool.Add(entityId, default);
            }
            return ref pool.Get(entityId);
        }
    }
}
