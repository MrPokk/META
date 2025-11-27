using Gley.Localization;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.UI.Button;

public class UIButtonProvider : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private WordIDs _wordID;
    [SerializeField] private ButtonManager _buttonManager;
    private ButtonClickedEvent _onClick;

    private void Awake()
    {
        InitializeComponents();
        UpdateUI();
    }

    private void OnValidate()
    {
        UpdateUIEditor();
    }

    private void InitializeComponents()
    {
        _buttonManager ??= GetComponent<ButtonManager>();
        _onClick ??= new ButtonClickedEvent();

        SetText(API.GetText(_wordID));
    }

    public void AddListener(UnityAction value)
    {
        _onClick?.AddListener(value);
    }

    public void RemoveListener(UnityAction value)
    {
        _onClick?.RemoveListener(value);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _onClick?.Invoke();
    }

    public void SetText(string valueText)
    {
        _buttonManager?.SetText(valueText);
    }

    public void UpdateUI()
    {
        _buttonManager?.UpdateUI();
    }

    private void UpdateUIEditor()
    {
        _buttonManager ??= GetComponent<ButtonManager>();
        _buttonManager?.SetText(_wordID.ToString());
        _buttonManager?.UpdateUI();
    }

}
