using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace BitterECS.Core
{
    public struct EntityBuilder<T> where T : EcsEntity
    {
        private readonly EcsPresenter _presenter;
        private Action<T> _postInitCallback;
        private Action<T> _preInitCallback;
        private ComponentAddOperations _componentAddOps;
        private ComponentAddedCallbacks _componentAddedCallbacks;
        private ILinkableView _linkableView;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal EntityBuilder(EcsPresenter presenter)
        {
            _presenter = presenter;
            _postInitCallback = null;
            _preInitCallback = null;
            _componentAddOps = default;
            _componentAddedCallbacks = default;
            _linkableView = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBuilder<T> WithLink(ILinkableView view)
        {
            _linkableView = view;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBuilder<T> WithPostInitCallback(Action<T> callback)
        {
            _postInitCallback = callback;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBuilder<T> WithPreInitCallback(Action<T> initAction)
        {
            _preInitCallback = initAction;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBuilder<T> WithComponent<C>(C component) where C : struct
        {
            _componentAddOps.Add<C>(component, _presenter, ref _componentAddedCallbacks);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityBuilder<T> WithComponentAddedCallback<C>(Action<T, C> callback) where C : struct
        {
            _componentAddedCallbacks.Add<C>(callback);
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILinkableEntity CreateToLinkable()
        {
            return Create();
        }

        public T Create()
        {
            var entity = _presenter.CreateEntity<T>();
            _preInitCallback?.Invoke(entity);

            _componentAddOps.Execute(entity);

            LinkViewIfNeeded(entity);

            _postInitCallback?.Invoke(entity);

            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LinkViewIfNeeded(T entity)
        {
            if (_linkableView != null)
            {
                EcsLinker.Link(entity, _linkableView);
            }
        }

        private struct ComponentAddOperations
        {
            private ComponentAddOperation[] _operations;
            private int _count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add<C>(C component, EcsPresenter presenter, ref ComponentAddedCallbacks callbacks) where C : struct
            {
                if (_operations == null)
                {
                    _operations = new ComponentAddOperation[EcsConfig.EntityCallbackFactor];
                }
                else if (_count == _operations.Length)
                {
                    Array.Resize(ref _operations, _operations.Length * 2);
                }

                _operations[_count++] = new ComponentAddOperation
                {
                    ComponentType = typeof(C),
                    Component = component,
                    Presenter = presenter,
                    Callbacks = callbacks
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
            private static void ExecuteOperation(ref ComponentAddOperation op, T entity)
            {
                var method = typeof(ComponentAddOperations).GetMethod(nameof(AddComponentInternal),
                    BindingFlags.NonPublic | BindingFlags.Static);
                var generic = method.MakeGenericMethod(op.ComponentType);
                generic.Invoke(null, new object[] { entity, op.Component, op.Presenter, op.Callbacks });
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void AddComponentInternal<C>(T entity, object component, EcsPresenter presenter, ComponentAddedCallbacks callbacks) where C : struct
            {
                var pool = presenter.GetPool<C>();
                pool.Add(entity.Properties.Id, (C)component);
                callbacks.Invoke<C>(entity, (C)component);
            }

            private struct ComponentAddOperation
            {
                public Type ComponentType;
                public object Component;
                public EcsPresenter Presenter;
                public ComponentAddedCallbacks Callbacks;
            }
        }

        private struct ComponentAddedCallbacks
        {
            private Dictionary<Type, object> _callbacks;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Add<C>(Action<T, C> callback) where C : struct
            {
                if (_callbacks == null)
                {
                    _callbacks = new Dictionary<Type, object>(EcsConfig.EntityCallbackFactor);
                }

                if (!_callbacks.TryGetValue(typeof(C), out var list))
                {
                    list = new List<Action<T, C>>(EcsConfig.EntityCallbackFactor);
                    _callbacks[typeof(C)] = list;
                }

                ((List<Action<T, C>>)list).Add(callback);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke<C>(T entity, C component) where C : struct
            {
                if (_callbacks == null || !_callbacks.TryGetValue(typeof(C), out var list))
                    return;

                var typedList = (List<Action<T, C>>)list;
                for (int i = 0; i < typedList.Count; i++)
                {
                    typedList[i](entity, component);
                }
            }
        }
    }
}
