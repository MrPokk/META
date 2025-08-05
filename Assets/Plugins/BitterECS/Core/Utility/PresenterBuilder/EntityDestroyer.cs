using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BitterECS.Core
{
    public struct EntityDestroyer<T> where T : EcsEntity
    {
        private readonly EcsPresenter _presenter;
        private readonly T _entity;
        private Action<T> _preDestroyCallback;
        private Action<T> _postDestroyCallback;
        private ComponentRemoveOperations _componentRemoveOps;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntityDestroyer(EcsPresenter presenter, T entity)
        {
            _presenter = presenter;
            _entity = entity;
            _preDestroyCallback = null;
            _postDestroyCallback = null;
            _componentRemoveOps = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityDestroyer<T> WithPreDestroyCallback(Action<T> callback)
        {
            _preDestroyCallback = callback;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityDestroyer<T> WithPostDestroyCallback(Action<T> callback)
        {
            _postDestroyCallback = callback;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityDestroyer<T> RemoveComponent<C>() where C : struct
        {
            _componentRemoveOps.Add<C>(_presenter);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Destroy()
        {
            _preDestroyCallback?.Invoke(_entity);
            _componentRemoveOps.Execute(_entity);
            CleanupView();
            _presenter.DestroyEntity(_entity);
            _postDestroyCallback?.Invoke(_entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CleanupView()
        {
            if (_entity.Has<ViewComponent>())
            {
                _entity.Get<ViewComponent>().current?.Dispose();
                EcsLinker.Unlink(_entity);
            }
        }

        private struct ComponentRemoveOperations
        {
            private ComponentRemoveOperation[] _operations;
            private int _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add<C>(EcsPresenter presenter) where C : struct
            {
                if (_operations == null)
                {
                    _operations = new ComponentRemoveOperation[EcsConfig.EntityCallbackFactor];
                }
                else if (_count == _operations.Length)
                {
                    Array.Resize(ref _operations, _operations.Length * 2);
                }

                _operations[_count++] = new ComponentRemoveOperation
                {
                    ComponentType = typeof(C),
                    Presenter = presenter
                };
            }

            public void Execute(T entity)
            {
                for (int i = 0; i < _count; i++)
                {
                    ExecuteOperation(ref _operations[i], entity);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void ExecuteOperation(ref ComponentRemoveOperation op, T entity)
            {
                var method = typeof(ComponentRemoveOperations).GetMethod(nameof(RemoveComponentInternal),
                    BindingFlags.NonPublic | BindingFlags.Static);
                var generic = method.MakeGenericMethod(op.ComponentType);
                generic.Invoke(null, new object[] { entity, op.Presenter });
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void RemoveComponentInternal<C>(T entity, EcsPresenter presenter) where C : struct
            {
                var pool = presenter.GetPool<C>();
                if (pool.Has(entity.Properties.Id))
                {
                    pool.Remove(entity.Properties.Id);
                }
            }

            private struct ComponentRemoveOperation
            {
                public Type ComponentType;
                public EcsPresenter Presenter;
            }
        }
    }
}
