using System;
using System.Collections.Generic;
using BitterECS.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class UIEntryPoint : IStartable
{
    private readonly IObjectResolver _container;
    private UIRootManager _uiRootManager;

    [Inject]
    public UIEntryPoint(IObjectResolver container)
    {
        _container = container;
    }

    public void Start()
    {
        var allBinders = new Dictionary<Type, WindowBinder>();
        var allPrefabs = Resources.LoadAll<GameObject>(PathProject.UI);

        foreach (var prefab in allPrefabs)
        {
            var container = prefab.GetComponent<WindowBinder>();
            if (container != null)
            {
                var type = container.GetType();
                allBinders.TryAdd(type, container);
            }
        }

        _uiRootManager = UIFactory.CreateRootManager(allBinders, _container);
        _uiRootManager.OpenScreen<UIMainScreen>();
    }
}
