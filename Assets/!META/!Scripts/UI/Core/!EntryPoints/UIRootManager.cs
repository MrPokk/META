using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIRootManager : MonoBehaviour
{
    private static UIRootManager _instance;
    private WindowsContainer _uiContainer;
    private bool _isInitialized;

    public static UIRootManager Instance => _instance ??= FindOrCreateInstance();

    private static UIRootManager FindOrCreateInstance()
    {
        var found = FindFirstObjectByType<UIRootManager>();
        if (found)
        {
            return found;
        }

        var go = new GameObject(nameof(UIRootManager));
        return go.AddComponent<UIRootManager>();
    }

    public void Initialize(WindowsContainer windowsContainer)
    {
        if (_isInitialized)
        {
            return;
        }


        _uiContainer = windowsContainer;
        _instance = this;
        _isInitialized = true;

        CloseAllPopupsInstance();
        CloseScreenInstance();
    }

    private void OnDestroy()
    {
        if (_instance != this)
        {
            return;
        }

        CloseAllPopupsInstance();
        CloseScreenInstance();
        _instance = null;
    }

    public static IWindowBinder GetCurrentScreen => Instance?._uiContainer.OpenedScreenBinder;
    public static IReadOnlyList<IWindowBinder> GetCurrentPopups => Instance?._uiContainer.OpenedBinders.Values.ToList();

    public static void OpenScreen<T>() where T : UIScreen => Instance?.OpenScreenInstance<T>();
    public static void CloseScreen() => Instance?.CloseScreenInstance();
    public static void OpenPopup<T>() where T : UIPopup => Instance?.OpenPopupInstance<T>();
    public static void ClosePopup<T>() where T : UIPopup => Instance?.ClosePopupInstance<T>();
    public static void CloseAllPopups() => Instance?.CloseAllPopupsInstance();

    private void OpenScreenInstance<T>() where T : UIScreen
    {
        CloseScreenInstance();

        var binder = CreateAndBindWindow<T>(isScreen: true);
        if (binder == null)
        {
            return;
        }

        _uiContainer.OpenedScreenBinder = binder;
        binder.Open();
    }

    private void CloseScreenInstance()
    {
        _uiContainer?.OpenedScreenBinder?.Close();
        _uiContainer.OpenedScreenBinder = null;
    }

    private void OpenPopupInstance<T>() where T : UIPopup
    {
        CloseExistingPopup<T>();

        var binder = CreateAndBindWindow<T>(isScreen: false);
        _uiContainer.TryAddOpenedBinder(typeof(T), binder);
        binder?.Open();
    }

    private void ClosePopupInstance<T>() where T : UIPopup
    {
        if (TryFindPopupInContainer<T>(out var existingPopup))
        {
            existingPopup.Close();
            _uiContainer.TryRemoveOpenedBinder(typeof(T));
        }
        else if (_uiContainer.OpenedBinders.TryGetValue(typeof(T), out var binder))
        {
            binder.Close();
            _uiContainer.TryRemoveOpenedBinder(typeof(T));
        }
    }

    private void CloseAllPopupsInstance()
    {

        foreach (var binder in _uiContainer.OpenedBinders.Values.ToList())
        {
            binder?.Close();
        }

        _uiContainer.ClearOpenedBinders();

        if (!_uiContainer.PopupsContainer)
        {
            return;
        }

        var countPopups = _uiContainer.PopupsContainer.childCount;
        for (int i = 0; i < countPopups; i++)
        {
            var child = _uiContainer.PopupsContainer.GetChild(i);
            if (child)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private bool TryFindPopupInContainer<T>(out T popup) where T : UIPopup
    {
        popup = _uiContainer?.PopupsContainer?.GetComponentsInChildren<T>()?.FirstOrDefault();
        return popup != null;
    }

    private void CloseExistingPopup<T>() where T : UIPopup
    {
        if (!TryFindPopupInContainer<T>(out var existingPopup))
        {
            return;
            
        }

        existingPopup.Close();
        _uiContainer.TryRemoveOpenedBinder(typeof(T));
    }

    private IWindowBinder CreateAndBindWindow<T>(bool isScreen) where T : WindowBinder
    {
        if (_uiContainer == null || !_uiContainer.TryGetBinder(typeof(T), out var binderPrefab))
        {
            Debug.LogError($"WindowBinder of type {typeof(T)} not found in binders dictionary");
            return null;
        }

        var parent = isScreen
        ? _uiContainer.ScreensContainer
        : _uiContainer.PopupsContainer;
        if (parent == null)
        {
            Debug.LogError($"Parent container for {(isScreen ? "screen" : "popup")} is null");
            return null;
        }

        var windowObject = Instantiate(binderPrefab.gameObject, parent);
        var binder = windowObject.GetComponent<IWindowBinder>();
        binder?.Bind(_uiContainer.RootContainer);

        return binder;
    }
}
