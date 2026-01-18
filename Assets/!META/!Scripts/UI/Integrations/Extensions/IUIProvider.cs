using UnityEngine.Events;
using UnityEngine.EventSystems;

public interface IUIProvider
{
    void AddListener(UnityAction action);
    void RemoveListener(UnityAction action);
}
