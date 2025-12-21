using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigationComponent : MonoBehaviour
{
    public void SetupNavigation(IList<UIButtonProvider> btnSelectables, bool circularNavigation)
    {
        if (!btnSelectables.Any())
        {
            return;
        }

        if (btnSelectables.Count == 1)
        {
            SetFirstSelectedButton(btnSelectables[0].gameObject);
            return;
        }

        for (var i = 0; i < btnSelectables.Count; i++)
        {
            GameObject upNeighbour = null;
            GameObject downNeighbour = null;

            if (btnSelectables.Count > 1)
            {
                if (circularNavigation)
                {
                    var prevIndex = (i - 1 + btnSelectables.Count) % btnSelectables.Count;
                    var nextIndex = (i + 1) % btnSelectables.Count;

                    upNeighbour = btnSelectables[prevIndex].gameObject;
                    downNeighbour = btnSelectables[nextIndex].gameObject;
                }
                else
                {
                    if (i > 0)
                    {
                        upNeighbour = btnSelectables[i - 1].gameObject;
                    }

                    if (i < btnSelectables.Count - 1)
                    {
                        downNeighbour = btnSelectables[i + 1].gameObject;
                    }
                }
            }

            btnSelectables[i].SetSelectNeighbours(upNeighbour, downNeighbour);
        }

        SetFirstSelectedButton(btnSelectables[0].gameObject);
    }

    public void SetFirstSelectedButton(GameObject btnSelectables)
    {
        EventSystem.current.SetSelectedGameObject(btnSelectables);
    }

    public static UINavigationComponent UsingNavigation(GameObject gameObject) =>
    gameObject.TryGetComponent(out UINavigationComponent component)
    ? component
    : gameObject.AddComponent<UINavigationComponent>();


    public UINavigationComponent ApplyNavigation(params UIButtonProvider[] btnSelectables)
    {
        SetupNavigation(btnSelectables, true);
        return this;
    }

    public UINavigationComponent ApplyNavigation(List<UIButtonProvider> btnSelectables, bool circularNavigation)
    {
        SetupNavigation(btnSelectables, circularNavigation);
        return this;
    }
}
