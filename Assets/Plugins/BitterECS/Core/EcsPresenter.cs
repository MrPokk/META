using System;
using System.Collections.Generic;

namespace BitterECS.Core
{
    public abstract class EcsPresenter : IDisposable
    {
        private ushort _nextEntityId;
        private readonly Dictionary<ushort, EcsEntity> _entities = new(EcsConfig.InitialEntitiesCapacity);
        private readonly Dictionary<Type, object> _pools = new(EcsConfig.InitialPoolCapacity);
        private readonly HashSet<Type> _allowedEntityTypes = new();
        
        public IReadOnlyCollection<EcsEntity> GetAll() => _entities.Values;

        protected EcsPresenter() => Registration();
        protected abstract void Registration();

        protected void AddLimitedType<T>() where T : EcsEntity => _allowedEntityTypes.Add(typeof(T));
        public bool IsTypeAllowed(Type type) => type.IsSubclassOf(typeof(EcsEntity)) && _allowedEntityTypes.Contains(type);
        public bool IsTypeAllowed<T>() where T : EcsEntity => _allowedEntityTypes.Contains(typeof(T));

        public void GetEntity(ushort id, out EcsEntity entity) => _entities.TryGetValue(id, out entity);

        public void AddEntity(EcsEntity entity) => CreateEntity(entity);
        public EcsEntity AddEntity(Type type) => CreateEntity(type);
        public EntityBuilder<T> AddEntity<T>() where T : EcsEntity => new(this);
        public EntityDestroyer<T> RemoveEntity<T>(T entity) where T : EcsEntity => new(this, entity);
        public void RemoveEntity(EcsEntity entity) => DestroyEntity(entity);

        internal ushort CreateEntity(EcsEntity entity)
        {
            entity.Init(new(this, ++_nextEntityId));
            entity.Registration();
            _entities.Add(_nextEntityId, entity);
            return _nextEntityId;
        }

        internal EcsEntity CreateEntity(Type type)
        {
            var entity = (EcsEntity)Activator.CreateInstance(type);
            entity.Init(new(this, ++_nextEntityId));
            entity.Registration();
            _entities.Add(_nextEntityId, entity);
            return entity;
        }

        internal T CreateEntity<T>() where T : EcsEntity
        {
            var entity = Activator.CreateInstance<T>();
            entity.Init(new(this, ++_nextEntityId));
            entity.Registration();
            _entities.Add(_nextEntityId, entity);
            return entity;
        }

        internal void DestroyEntity(EcsEntity entity)
        {
            if (_entities.Remove(entity.Properties.Id, out _))
                entity.Dispose();
        }

        public EcsFilter Filter() => new(this);

        public EcsPool<T> GetPool<T>() where T : struct => 
            (EcsPool<T>)(_pools.TryGetValue(typeof(T), out var pool) 
                ? pool 
                : _pools[typeof(T)] = new EcsPool<T>());

        public void Dispose()
        {
            foreach (var entity in _entities.Values) entity.Dispose();
            foreach (var pool in _pools.Values) (pool as IDisposable)?.Dispose();
            
            _entities.Clear();
            _pools.Clear();
            _allowedEntityTypes.Clear();
            
            GC.SuppressFinalize(this);
        }
    }
}
