using UnityEngine;
using UnityEngine.UI;

public class UIPopup : WindowBinder
{
    [SerializeField] private UIButtonProvider _btnClose;
    [SerializeField] private UIButtonProvider _btnAlternativeClose;

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
        if (_btnClose) _btnClose.AddListener(OnCloseClicked);
        if (_btnAlternativeClose) _btnAlternativeClose.AddListener(OnCloseClicked);
    }

    private void RemoveListeners()
    {
        if (_btnClose) _btnClose.RemoveListener(OnCloseClicked);
        if (_btnAlternativeClose) _btnAlternativeClose.RemoveListener(OnCloseClicked);
    }

    private void OnCloseClicked() => Close();
}
