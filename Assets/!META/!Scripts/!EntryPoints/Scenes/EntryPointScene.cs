using Mirror;
using VContainer;
using VContainer.Unity;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntryPointScene : LifetimeScope
{
    protected IObjectResolver ParentContainer => Parent.Container;
    protected override void Awake()
    {
        if (NetworkServer.active && !NetworkClient.active)
        {
            Destroy(gameObject);
            return;
        }


        autoRun = false;
        parentReference = ParentReference.Create<EntryPointProject>();
        base.Awake();
    }

    private void Start()
    {
        if (NetworkServer.active && !NetworkClient.active)
        {
            Destroy(gameObject);
            return;
        }

        Container.Inject(gameObject);

        // Выполняем инъекцию для всех объектов из списка autoInjectGameObjects
        if (autoInjectGameObjects != null)
        {
            foreach (var injectGameObject in autoInjectGameObjects)
            {
                if (injectGameObject != null)
                {
                    Container.InjectGameObject(injectGameObject);
                }
            }
        }

        Bootstrap();
    }

    protected abstract void Bootstrap();
}
