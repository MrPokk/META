using System;
using System.Collections.Generic;
using BitterECS.Extra;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class UIEntryPoint : IStartable
{
    private readonly IObjectResolver _container;

    [Inject]
    public UIEntryPoint(IObjectResolver container) => _container = container;

    public void Start()
    {
        var allBinders = new Dictionary<Type, WindowBinder>();
        var allPrefabs = Resources.LoadAll<GameObject>(PathProject.UI);

        foreach (var prefab in allPrefabs)
        {
            if (prefab.TryGetComponent<WindowBinder>(out var binder))
            {
                var type = binder.GetType();
                allBinders.TryAdd(type, binder);
            }
        }

        UIFactory.CreateRootManager(allBinders, _container);
    }
}
