using System;
using UnityEngine;

public class UICornerPopup : UIPopup
{
    [SerializeField] private UIButtonProvider _btnGoToChat;
    [SerializeField] private UIButtonProvider _btnGoToSettings;
    [SerializeField] private UIButtonProvider _btnGoToInventory;
    [SerializeField] private UIButtonProvider _btnGoToMenu;

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
        _btnGoToChat.AddListener(OnGoToChatButted);
        _btnGoToSettings.AddListener(OnGoToSettingsButted);
        _btnGoToInventory.AddListener(OnGoToInventoryButted);
        _btnGoToMenu.AddListener(OnGoToMenuButted);
    }

    private void OnGoToMenuButted()
    {
        UIRootManager.OpenPopup<UIMainMenuPopup>();
    }

    private void OnGoToChatButted()
    {
        UIRootManager.ChangePopup<UIChatPopup>();
    }

    private void OnGoToInventoryButted()
    {
        UIRootManager.OpenPopup<UIComingSoonPopup>();
    }

    private void OnGoToSettingsButted()
    {
        UIRootManager.OpenPopup<UISettingPopup>();
    }
}
