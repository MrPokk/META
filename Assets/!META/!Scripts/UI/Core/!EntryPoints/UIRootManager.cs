using UnityEngine;

public class UIRootManager : MonoBehaviour
{
    private static UIRootManager s_instance;
    public static UIRootManager Instance => s_instance;

    private WindowsContainer _windowsContainer;

    public void Initialize(WindowsContainer windowsContainer)
    {
        _windowsContainer = windowsContainer;
        s_instance = this;
    }

    public void OpenScreen<T>() where T : WindowBinder
    {
        var binder = Binding<T>();

        _windowsContainer.OpenedScreenBinder?.Close();
        _windowsContainer.OpenedScreenBinder = binder;
        _windowsContainer.OpenedScreenBinder.Open();
    }

    public void CloseScreen()
    {
        _windowsContainer.OpenedScreenBinder?.Close();
        _windowsContainer.OpenedScreenBinder = null;
    }

    public void OpenPopup<T>() where T : WindowBinder
    {
        var binder = Binding<T>();

        _windowsContainer.OpenedBinders[typeof(T)] = binder;
        binder.Open();
    }

    public void ClosePopup<T>() where T : WindowBinder
    {
        if (_windowsContainer.OpenedBinders.TryGetValue(typeof(T), out var binder))
        {
            binder.Close();
            _windowsContainer.OpenedBinders.Remove(typeof(T));
        }
    }

    public void CloseAllPopups()
    {
        foreach (var binder in _windowsContainer.OpenedBinders.Values)
        {
            binder.Close();
        }
        _windowsContainer.OpenedBinders.Clear();
    }

    private IWindowBinder Binding<T>() where T : WindowBinder
    {
        var binderPrefab = _windowsContainer.Binders[typeof(T)];
        var popup = Instantiate(binderPrefab.gameObject, _windowsContainer.PopupsContainer);
        var binder = popup.GetComponent<IWindowBinder>();
        binder.Bind(_windowsContainer.RootContainer);

        return binder;
    }
}
