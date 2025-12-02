using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public abstract class EntryPointFloors : LifetimeScope
{
    [SerializeField] protected List<InjectorToChildren> _injectorToChildren;
    [SerializeField] protected IsPlayerSpawnPoint _playerSpawnPoint;

    public Vector3 PlayerSpawnPoint
    {
        get
        {
            if (_playerSpawnPoint == null) throw new NullReferenceException("PlayerSpawnPoint is null");
            return _playerSpawnPoint.transform.position;
        }
    }

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterBuildCallback(container =>
        {
            foreach (var injector in _injectorToChildren)
            {
                if (injector == null) continue;

                injector.Bind(container);
            }
        });
    }
}
