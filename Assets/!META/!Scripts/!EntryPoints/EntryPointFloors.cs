using System.Collections.Generic;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public abstract class EntryPointFloors : LifetimeScope
{
    [SerializeField] protected List<InjectorToChildren> _injectorToChildren;
    [SerializeField] protected IsPlayerSpawnPoint _playerSpawnPoint;

    public Vector3 PlayerSpawnPoint => _playerSpawnPoint.transform.position;
    public Vector3 PlayerSpawnRotationForward => _playerSpawnPoint.transform.forward;

    protected override void Awake()
    {
        parentReference = ParentReference.Create<EntryPointProject>();
        CursorService.LockCursor();
        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        InjectToCallback(builder);
    }

    private void InjectToCallback(IContainerBuilder builder)
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

    private void OnDrawGizmos()
    {
        var ray = new Ray(_playerSpawnPoint.transform.position, Vector3.down);
        var position = _playerSpawnPoint.transform.position;
        var forward = _playerSpawnPoint.transform.forward;
        var rayForwardRotation = new Ray(position, forward);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(ray);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(rayForwardRotation);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, 0.5f);
    }
}
