using System;
using UnityEngine;

public class UICornerPopup : UIPopup
{
    [SerializeField] private UIButtonProvider _btnGoToProfile;
    [SerializeField] private UIButtonProvider _btnGoToSettings;
    [SerializeField] private UIButtonProvider _btnGoToInventory;

    public override void Open()
    {
        AddListener();

        UIAnimationComponent
            .UsingAnimation(gameObject)
            .ApplyPresetOpen(UIAnimationPresets.CreateSlideFromRightPreset())
            .PlayOpenAnimation();

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
        UIRootManager.OpenPopup<UISettingPopup>();
    }
}
