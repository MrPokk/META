using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigationComponent : MonoBehaviour
{
    private IList<UIButtonProvider> _buttonProviders;
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
        _buttonProviders = btnSelectable;
        for (var i = 0; i < btnSelectable.Count; i++)
        {
            GameObject upNeighbour = null;
            GameObject downNeighbour = null;

            if (btnSelectable.Count > 1)
            {
                if (circularNavigation)
                {
                    var prevIndex = (i - 1 + btnSelectable.Count) % btnSelectable.Count;
                    var nextIndex = (i + 1) % btnSelectable.Count;

                    upNeighbour = btnSelectable[prevIndex].gameObject;
                    downNeighbour = btnSelectable[nextIndex].gameObject;
                }
                else
                {
                    if (i > 0)
                    {
                        upNeighbour = btnSelectable[i - 1].gameObject;
                    }

                    if (i < btnSelectable.Count - 1)
                    {
                        downNeighbour = btnSelectable[i + 1].gameObject;
                    }
                }
                btnSelectable[i].SetSelectNeighbours(upNeighbour, downNeighbour);
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
