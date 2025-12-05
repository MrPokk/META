using Michsky.MUIP;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Button;

[RequireComponent(typeof(ButtonManager))]
public class UIButtonProvider : LocalizedUIElement, IPointerClickHandler
{
    [SerializeField] private ButtonManager _buttonManager;
    private ButtonClickedEvent _onClick;

    protected override void InitializeComponents()
    {
        _buttonManager ??= GetComponent<ButtonManager>();
        _onClick ??= new ButtonClickedEvent();
    }

    public void AddListener(UnityEngine.Events.UnityAction action)
    {
        _onClick?.AddListener(action);
    }

    public void RemoveListener(UnityEngine.Events.UnityAction action)
    {
        _onClick?.RemoveListener(action);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();
    }

    public override void SetText(string text)
    {
        _buttonManager?.SetText(text);
        _buttonManager?.UpdateUI();
    }
}
