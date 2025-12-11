using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BitterECS.Core
{
    public interface IPoolDestroy
    {
        bool Has(int entityId);
        void Remove(int entityId);
    }

    public class EcsPool<T> : IDisposable, IPoolDestroy where T : struct
    {
        private T[] _components;
        private int[] _entityToDataIndex;
        private int[] _dataIndexToEntity;
        private bool[] _isFree;
        private int _count;
        private Stack<int> _freeIndices;
        private readonly int _initialCapacity;

        public EcsPool(int initialCapacity = -1)
        {
            _initialCapacity = initialCapacity > 0 ? initialCapacity : EcsConfig.InitialPoolCapacity;
            _components = Array.Empty<T>();
            _entityToDataIndex = Array.Empty<int>();
            _dataIndexToEntity = Array.Empty<int>();
            _isFree = Array.Empty<bool>();
            _freeIndices = new Stack<int>(_initialCapacity);
            _count = 0;
        }

        public void Add(int entityId, in T component)
        {
            if (entityId >= _entityToDataIndex.Length)
            {
                var oldLength = _entityToDataIndex.Length;
                var newSize = oldLength == 0
                    ? Math.Max(entityId + 1, _initialCapacity)
                    : Math.Max(entityId + 1, oldLength * EcsConfig.PoolGrowthFactor);

                Array.Resize(ref _entityToDataIndex, newSize);

                for (int i = oldLength; i < newSize; i++)
                {
                    _entityToDataIndex[i] = -1;
                }
            }

            if (_entityToDataIndex[entityId] != -1)
                return;

            int dataIndex;
            if (_freeIndices.Count > 0)
            {
                dataIndex = _freeIndices.Pop();
                _isFree[dataIndex] = false;
            }
            else
            {
                if (_count >= _components.Length)
                {
                    var newCapacity = _components.Length == 0
                        ? _initialCapacity
                        : _components.Length * EcsConfig.PoolGrowthFactor;

                    Array.Resize(ref _components, newCapacity);
                    Array.Resize(ref _dataIndexToEntity, newCapacity);
                    Array.Resize(ref _isFree, newCapacity);
                }
                dataIndex = _count++;
                _isFree[dataIndex] = false;
            }

            _components[dataIndex] = component;
            _entityToDataIndex[entityId] = dataIndex;
            _dataIndexToEntity[dataIndex] = entityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int entityId)
        {
            if (entityId >= _entityToDataIndex.Length || _entityToDataIndex[entityId] == -1)
            {
                throw new KeyNotFoundException($"Entity {entityId} doesn't have this component");
            }

            return ref _components[_entityToDataIndex[entityId]];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int entityId)
        {
            return entityId < _entityToDataIndex.Length && _entityToDataIndex[entityId] != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(int entityId, out T component)
        {
            if (entityId < _entityToDataIndex.Length && _entityToDataIndex[entityId] != -1)
            {
                component = _components[_entityToDataIndex[entityId]];
                return true;
            }

            component = default;
            return false;
        }

        public void Remove(int entityId)
        {
            if (entityId >= _entityToDataIndex.Length || _entityToDataIndex[entityId] == -1)
            {
                return;
            }

            var dataIndex = _entityToDataIndex[entityId];
            _entityToDataIndex[entityId] = -1;

            if (dataIndex < _count - 1)
            {
                _components[dataIndex] = _components[_count - 1];
                _dataIndexToEntity[dataIndex] = _dataIndexToEntity[_count - 1];

                var movedEntityId = _dataIndexToEntity[dataIndex];
                _entityToDataIndex[movedEntityId] = dataIndex;

                _components[_count - 1] = default;
                _dataIndexToEntity[_count - 1] = -1;
                _isFree[_count - 1] = true;
            }
            else
            {
                _components[dataIndex] = default;
                _dataIndexToEntity[dataIndex] = -1;
            }

            _isFree[dataIndex] = true;
            _freeIndices.Push(dataIndex);
            _count--;
        }

        public int Count => _count - _freeIndices.Count;
        public int Capacity => _components.Length;

        public void Clear()
        {
            if (_count > 0)
            {
                Array.Clear(_components, 0, _count);
                Array.Clear(_dataIndexToEntity, 0, _count);
                Array.Clear(_isFree, 0, _count);
            }

            for (int i = 0; i < _entityToDataIndex.Length; i++)
            {
                _entityToDataIndex[i] = -1;
            }

            _count = 0;
            _freeIndices.Clear();
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity > _components.Length)
            {
                int newCapacity = Math.Max(capacity, _components.Length * EcsConfig.PoolGrowthFactor);
                Array.Resize(ref _components, newCapacity);
                Array.Resize(ref _dataIndexToEntity, newCapacity);
                Array.Resize(ref _isFree, newCapacity);
            }
        }

        public void TrimExcess()
        {
            var occupiedCount = _count - _freeIndices.Count;
            if (occupiedCount < _components.Length * 0.9)
            {
                var newComponents = new T[occupiedCount];
                var newDataIndexToEntity = new int[occupiedCount];
                var newIsFree = new bool[occupiedCount];

                int newIndex = 0;
                for (int i = 0; i < _count; i++)
                {
                    if (!_isFree[i])
                    {
                        newComponents[newIndex] = _components[i];
                        newDataIndexToEntity[newIndex] = _dataIndexToEntity[i];
                        newIsFree[newIndex] = false;

                        _entityToDataIndex[newDataIndexToEntity[newIndex]] = newIndex;

                        newIndex++;
                    }
                }

                _components = newComponents;
                _dataIndexToEntity = newDataIndexToEntity;
                _isFree = newIsFree;
                _count = occupiedCount;
                _freeIndices.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<T> AsOccupiedSpan()
        {
            return _components.AsSpan(0, _count);
        }

        public IEnumerable<int> GetEntityIds()
        {
            for (int i = 0; i < _count; i++)
            {
                if (!_isFree[i])
                {
                    yield return _dataIndexToEntity[i];
                }
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }

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
