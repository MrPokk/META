using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigationComponent : MonoBehaviour
{
    private List<UIButtonProvider> _buttonProviders;
    private bool _isFirstSelected;


    private void SetupNavigation(IList<UIButtonProvider> btnSelectable, bool circularNavigation)
    {
        if (!btnSelectable.Any())
        {
            return;
        }

        FindButtonSelected(btnSelectable, circularNavigation);

        if (_isFirstSelected)
        {
            SetSelectedButton(_buttonProviders[0].gameObject);
        }
    }

    private void FindButtonSelected(IList<UIButtonProvider> btnSelectable, bool circularNavigation)
    {
        _buttonProviders = new List<UIButtonProvider>(btnSelectable);

        for (var i = 0; i < _buttonProviders.Count; i++)
        {
            GameObject upNeighbour = null;
            GameObject downNeighbour = null;

            if (_buttonProviders.Count > 1)
            {
                if (circularNavigation)
                {
                    var prevIndex = (i - 1 + _buttonProviders.Count) % btnSelectable.Count;
                    var nextIndex = (i + 1) % _buttonProviders.Count;

                    upNeighbour = _buttonProviders[prevIndex].gameObject;
                    downNeighbour = _buttonProviders[nextIndex].gameObject;
                }
                else
                {
                    if (i > 0)
                    {
                        upNeighbour = _buttonProviders[i - 1].gameObject;
                    }

                    if (i < _buttonProviders.Count - 1)
                    {
                        downNeighbour = _buttonProviders[i + 1].gameObject;
                    }
                }
                _buttonProviders[i].SetSelectNeighbours(upNeighbour, downNeighbour);
            }
        }

        foreach (var btn in _buttonProviders)
        {
            btn.UpdateUI();
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
        SetupNavigation(btnSelectable, true);
    }

    public void ApplyNavigation(List<UIButtonProvider> btnSelectable, bool circularNavigation)
    {
        SetupNavigation(btnSelectable, circularNavigation);
    }
}
