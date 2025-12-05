using UnityEngine;
using VContainer;

public abstract class WindowBinder : MonoBehaviour, IWindowBinder
{
    protected IObjectResolver Container { get; private set; }

    public void Bind(IObjectResolver resolver)
    {
        Container = resolver;
        Container.Inject(this);
    }

    public virtual void Open()
    {
        if (this == null || gameObject == null)
        {
            return;
        }

        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (this == null || gameObject == null) return;
        gameObject.SetActive(false);

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}

