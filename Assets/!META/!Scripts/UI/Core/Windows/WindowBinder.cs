using UnityEngine;
using VContainer;

public abstract class WindowBinder : MonoBehaviour, IWindowBinder
{
    protected IObjectResolver Container { get; private set; }

    public IWindowBinder Bind(IObjectResolver resolver)
    {
        Container = resolver;
        Container.Inject(this);
        return this;
    }

    public virtual void Open()
    {
        if (!this || !gameObject) return;
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (!this || !gameObject) return;
        gameObject.SetActive(false);

        if (this && gameObject && !gameObject.activeSelf)
        {
            Destroy(gameObject);
        }
    }
}
