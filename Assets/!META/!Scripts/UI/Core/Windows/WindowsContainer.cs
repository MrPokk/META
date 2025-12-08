using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class WindowsContainer
{
    public readonly IObjectResolver RootContainer;
    public readonly Transform ScreensContainer;
    public readonly Transform PopupsContainer;
    
    private readonly Dictionary<Type, IWindowBinder> _openedBinders = new();
    private readonly Dictionary<Type, WindowBinder> _allBinders;
    
    public IWindowBinder OpenedScreenBinder { get; set; }
    public IReadOnlyDictionary<Type, WindowBinder> AllBinders => _allBinders;
    public IReadOnlyDictionary<Type, IWindowBinder> OpenedBinders => _openedBinders;

    public WindowsContainer(
        IObjectResolver rootContainer,
        Transform popupsContainer,
        Transform screensContainer,
        Dictionary<Type, WindowBinder> binders)
    {
        RootContainer = rootContainer;
        PopupsContainer = popupsContainer;
        ScreensContainer = screensContainer;
        _allBinders = binders;
    }
    
    public bool TryGetBinder(Type type, out WindowBinder binder) => 
        _allBinders.TryGetValue(type, out binder);
    
    public bool IsPopupOpen(Type type) => _openedBinders.ContainsKey(type);
    
    public bool TryAddOpenedBinder(Type type, IWindowBinder binder) => 
        _openedBinders.TryAdd(type, binder);
    
    public bool TryRemoveOpenedBinder(Type type) => 
        _openedBinders.Remove(type);
    
    public void ClearOpenedBinders() => _openedBinders.Clear();
}
