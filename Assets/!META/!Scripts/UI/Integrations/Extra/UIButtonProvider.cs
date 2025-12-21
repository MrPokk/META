using Michsky.MUIP;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

[RequireComponent(typeof(ButtonManager))]
public class UIButtonProvider : LocalizedUIElement, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] private ButtonManager _buttonManager;
    private ButtonClickedEvent _onSubmit;

    protected override void InitializeComponents()
    {
        _buttonManager ??= GetComponent<ButtonManager>();
        _onSubmit ??= new ButtonClickedEvent();

        _buttonManager.useUINavigation = true;
        _buttonManager.navigationMode = Navigation.Mode.Explicit;
    }

    public void SetSelectNeighbours(GameObject selectOnUp, GameObject selectOnDown)
    {
        _buttonManager.selectOnUp = selectOnUp;
        _buttonManager.selectOnDown = selectOnDown;
    }

    public void AddListener(UnityEngine.Events.UnityAction action)
    {
        _onSubmit?.AddListener(action);
    }

    public void RemoveListener(UnityEngine.Events.UnityAction action)
    {
        _onSubmit?.RemoveListener(action);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        _onSubmit?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onSubmit?.Invoke();
    }

    public override void SetText(string text)
    {
        _buttonManager?.SetText(text);
        _buttonManager?.UpdateUI();
    }

    public void SelectButton()
    {
        if (_buttonManager != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

}
