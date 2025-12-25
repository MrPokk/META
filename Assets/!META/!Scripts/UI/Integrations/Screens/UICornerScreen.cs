using System;
using UnityEngine;

public class UICornerScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToProfile;
    [SerializeField] private UIButtonProvider _btnGoToSettings;
    [SerializeField] private UIButtonProvider _btnGoToInventory;

    public override void Open()
    {
        AddListener();

        base.Open();
    }

    private void AddListener()
    {
        _btnGoToProfile.AddListener(OnGoToProfileButted);
        _btnGoToSettings.AddListener(OnGoToSettingsButted);
        _btnGoToInventory.AddListener(OnGoToInventoryButted);
    }

    private void OnGoToProfileButted()
    {
    }

    private void OnGoToInventoryButted()
    {
    }

    private void OnGoToSettingsButted()
    {
    }
}
