using UnityEngine;
using UnityEngine.UI;

public class UIPopup : WindowBinder
{
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnAlternativeClose;

    public override void Open()
    {
        AddListeners();
        base.Open();
    }

    public override void Close()
    {
        RemoveListeners();
        base.Close();
    }

    private void AddListeners()
    {
        if (_btnClose) _btnClose.onClick.AddListener(OnCloseClicked);
        if (_btnAlternativeClose) _btnAlternativeClose.onClick.AddListener(OnCloseClicked);
    }

    private void RemoveListeners()
    {
        if (_btnClose) _btnClose.onClick.RemoveListener(OnCloseClicked);
        if (_btnAlternativeClose) _btnAlternativeClose.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked() => Close();
}
