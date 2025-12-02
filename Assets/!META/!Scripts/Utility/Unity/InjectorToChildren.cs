using UnityEngine;
using VContainer;
using VContainer.Unity;

public class InjectorToChildren : MonoBehaviour
{
    public void Bind(IObjectResolver container)
    {
        InjectObject(container);
    }

    private void InjectObject(IObjectResolver container)
    {
        var obj = GetComponentsInChildren<Transform>();
        foreach (var item in obj)
        {
            container.InjectGameObject(item.gameObject);
        }
    }
}
