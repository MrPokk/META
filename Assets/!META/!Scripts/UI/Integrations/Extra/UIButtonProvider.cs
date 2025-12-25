using System;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;
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

        _buttonManager.onHover.AddListener(OnHover);

        _buttonManager.useUINavigation = true;
        _buttonManager.navigationMode = Navigation.Mode.Explicit;
    }

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    private void OnHover()
    {
        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void SetSelectNeighbours(GameObject selectOnUp, GameObject selectOnDown)
    {
        SetSelectNeighbours(selectOnUp, selectOnDown, null, null);
    }

    public void SetSelectNeighbours(GameObject selectOnUp, GameObject selectOnDown,
                               GameObject selectOnLeft, GameObject selectOnRight)
    {
        _buttonManager.selectOnUp = selectOnUp;
        _buttonManager.selectOnDown = selectOnDown;
        _buttonManager.selectOnLeft = selectOnLeft;
        _buttonManager.selectOnRight = selectOnRight;
    }

    public void AddListener(UnityAction action)
    {
        _onSubmit?.AddListener(action);
    }

    public void RemoveListener(UnityAction action)
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
        gameObject.name = $"BtnUI_{WordIDString}";
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

    public void UpdateUI()
    {
        _buttonManager.UpdateUI();
    }

    private void OnDestroy()
    {
        _onSubmit?.RemoveAllListeners();
        _buttonManager.onHover.RemoveAllListeners();
    }
}
