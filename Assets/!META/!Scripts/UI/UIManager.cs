using UnityEngine;
using System.Collections.Generic;
using System;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    public static UIManager Instance => _instance;

    [SerializeField] private Transform _screensParent;
    [SerializeField] private Transform _popupsParent;

    private readonly Dictionary<Type, UIScreen> _screens = new();
    private readonly Dictionary<Type, UIPopup> _popups = new();
    private readonly Stack<UIScreen> _screenStack = new();
    private readonly List<UIPopup> _activePopups = new();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        InitializeUI();
    }

    private void InitializeUI()
    {
        // Инициализация экранов
        foreach (Transform child in _screensParent)
        {
            var screen = child.GetComponent<UIScreen>();
            if (screen != null)
            {
                var screenType = screen.GetType();
                if (!_screens.ContainsKey(screenType))
                {
                    _screens.Add(screenType, screen);
                    screen.Initialize();
                    screen.Hide();
                }
            }
        }

        // Инициализация pop-up окон
        foreach (Transform child in _popupsParent)
        {
            var popup = child.GetComponent<UIPopup>();
            if (popup != null)
            {
                System.Type popupType = popup.GetType();
                if (!_popups.ContainsKey(popupType))
                {
                    _popups.Add(popupType, popup);
                    popup.Initialize();
                    popup.Hide();
                }
            }
        }
    }

    #region Screen Management
    public T GetScreen<T>() where T : UIScreen
    {
        if (_screens.TryGetValue(typeof(T), out UIScreen screen))
        {
            return (T)screen;
        }
        return null;
    }

    public void ShowScreen<T>(object data = null) where T : UIScreen
    {
        if (_screens.TryGetValue(typeof(T), out UIScreen screen))
        {
            if (_screenStack.Count > 0)
            {
                var currentScreen = _screenStack.Peek();
                currentScreen.Hide();
            }

            screen.Show(data);
            _screenStack.Push(screen);
        }
    }

    public void HideCurrentScreen()
    {
        if (_screenStack.Count == 0) return;

        var screen = _screenStack.Pop();
        screen.Hide();

        if (_screenStack.Count > 0)
        {
            var previousScreen = _screenStack.Peek();
            previousScreen.Show();
        }
    }
    #endregion

    #region Popup Management
    public T GetPopup<T>() where T : UIPopup
    {
        if (_popups.TryGetValue(typeof(T), out UIPopup popup))
        {
            return (T)popup;
        }
        return null;
    }

    public void ShowPopup<T>(object data = null) where T : UIPopup
    {
        if (_popups.TryGetValue(typeof(T), out UIPopup popup))
        {
            popup.Show(data);
            _activePopups.Add(popup);
        }
    }

    public void HidePopup<T>() where T : UIPopup
    {
        if (_popups.TryGetValue(typeof(T), out UIPopup popup))
        {
            popup.Hide();
            _activePopups.Remove(popup);
        }
    }

    public void HideAllPopups()
    {
        foreach (var popup in _activePopups)
        {
            popup.Hide();
        }
        _activePopups.Clear();
    }
    #endregion
}
