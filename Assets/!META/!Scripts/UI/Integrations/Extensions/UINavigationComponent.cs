using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigationComponent : MonoBehaviour
{
    private List<UIButtonProvider> _buttonProviders;
    private bool _isFirstSelected;

    public enum NavigationMode
    {
        Vertical,
        Horizontal,
        Both //TODO: Implement Both
    }

    private void SetupNavigation(IList<UIButtonProvider> btnSelectable, 
                               bool circularNavigation, 
                               NavigationMode navigationMode = NavigationMode.Vertical)
    {
        if (!btnSelectable.Any())
        {
            return;
        }

        FindButtonSelected(btnSelectable, circularNavigation, navigationMode);

        if (_isFirstSelected)
        {
            SetSelectedButton(_buttonProviders[0].gameObject);
        }
    }

    private void FindButtonSelected(IList<UIButtonProvider> btnSelectable, 
                                   bool circularNavigation, 
                                   NavigationMode navigationMode)
    {
        _buttonProviders = new List<UIButtonProvider>(btnSelectable);

        for (var i = 0; i < _buttonProviders.Count; i++)
        {
            GameObject upNeighbour = null;
            GameObject downNeighbour = null;
            GameObject leftNeighbour = null;
            GameObject rightNeighbour = null;

            if (_buttonProviders.Count > 1)
            {
                switch (navigationMode)
                {
                    case NavigationMode.Vertical:
                        SetupVerticalNavigation(i, circularNavigation, ref upNeighbour, ref downNeighbour);
                        break;
                    
                    case NavigationMode.Horizontal:
                        SetupHorizontalNavigation(i, circularNavigation, ref leftNeighbour, ref rightNeighbour);
                        break;
                    
                    case NavigationMode.Both:
                         //TODO: Implement both navigation
                        break;
                }
                
                _buttonProviders[i].SetSelectNeighbours(upNeighbour, downNeighbour, leftNeighbour, rightNeighbour);
            }
        }

        foreach (var btn in _buttonProviders)
        {
            btn.UpdateUI();
        }
    }

    private void SetupVerticalNavigation(int index, bool circularNavigation, 
                                        ref GameObject upNeighbour, ref GameObject downNeighbour)
    {
        if (circularNavigation)
        {
            var prevIndex = (index - 1 + _buttonProviders.Count) % _buttonProviders.Count;
            var nextIndex = (index + 1) % _buttonProviders.Count;

            upNeighbour = _buttonProviders[prevIndex].gameObject;
            downNeighbour = _buttonProviders[nextIndex].gameObject;
        }
        else
        {
            if (index > 0)
            {
                upNeighbour = _buttonProviders[index - 1].gameObject;
            }

            if (index < _buttonProviders.Count - 1)
            {
                downNeighbour = _buttonProviders[index + 1].gameObject;
            }
        }
    }

    private void SetupHorizontalNavigation(int index, bool circularNavigation, 
                                          ref GameObject leftNeighbour, ref GameObject rightNeighbour)
    {
        if (circularNavigation)
        {
            var prevIndex = (index - 1 + _buttonProviders.Count) % _buttonProviders.Count;
            var nextIndex = (index + 1) % _buttonProviders.Count;

            leftNeighbour = _buttonProviders[prevIndex].gameObject;
            rightNeighbour = _buttonProviders[nextIndex].gameObject;
        }
        else
        {
            if (index > 0)
            {
                leftNeighbour = _buttonProviders[index - 1].gameObject;
            }

            if (index < _buttonProviders.Count - 1)
            {
                rightNeighbour = _buttonProviders[index + 1].gameObject;
            }
        }
    }

    private void SetSelectedButton(GameObject btnSelectable)
    {
        EventSystem.current.SetSelectedGameObject(btnSelectable);
    }

    public void SetFirstSelectedButton()
    {
        SetSelectedButton(_buttonProviders[0].gameObject);
    }

    public static UINavigationComponent UsingNavigation(GameObject gameObject) =>
        gameObject.TryGetComponent(out UINavigationComponent component)
            ? component
            : gameObject.AddComponent<UINavigationComponent>();

    public UINavigationComponent ApplySelected(int index)
    {
        _isFirstSelected = false;
        SetSelectedButton(_buttonProviders[index].gameObject);
        return this;
    }

    public UINavigationComponent ApplyFirstSelected()
    {
        _isFirstSelected = true;
        return this;
    }

    public void ApplyNavigation(params UIButtonProvider[] btnSelectable)
    {
        SetupNavigation(btnSelectable, true, NavigationMode.Vertical);
    }

    public void ApplyNavigation(List<UIButtonProvider> btnSelectable, bool circularNavigation)
    {
        SetupNavigation(btnSelectable, circularNavigation, NavigationMode.Vertical);
    }

    public void ApplyNavigation(NavigationMode navigationMode, params UIButtonProvider[] btnSelectable)
    {
        SetupNavigation(btnSelectable, true, navigationMode);
    }

    public void ApplyNavigation(List<UIButtonProvider> btnSelectable, bool circularNavigation, 
                               NavigationMode navigationMode)
    {
        SetupNavigation(btnSelectable, circularNavigation, navigationMode);
    }

    public void ApplyVerticalNavigation(params UIButtonProvider[] btnSelectable)
    {
        ApplyNavigation(NavigationMode.Vertical, btnSelectable);
    }

    public void ApplyHorizontalNavigation(params UIButtonProvider[] btnSelectable)
    {
        ApplyNavigation(NavigationMode.Horizontal, btnSelectable);
    }

    public void ApplyGridNavigation(params UIButtonProvider[] btnSelectable)
    {
        ApplyNavigation(NavigationMode.Both, btnSelectable);
    }
}
