using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    [SerializeField] protected CanvasGroup _canvasGroup;

    public virtual void Initialize()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
    }

    public virtual void Show(object data = null)
    {
        gameObject.SetActive(true);
        _canvasGroup.alpha = 0;
    }

    public virtual void Hide()
    {

    }

    protected virtual void OnDestroy()
    {
    }
}
