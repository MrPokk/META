using UnityEngine;
using VContainer;

public abstract class WindowBinder : MonoBehaviour, IWindowBinder
{
    protected IObjectResolver Container { get; private set; }

    public void Bind(IObjectResolver resolver)
    {
        Container = resolver.Resolve<IObjectResolver>();
    }

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        Destroy(gameObject);
    }
}

