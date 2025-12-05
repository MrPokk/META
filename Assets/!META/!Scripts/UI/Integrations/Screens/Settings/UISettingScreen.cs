using System;
using UnityEngine;

public class UISettingScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToVideo;
    [SerializeField] private UIButtonProvider _btnGoToSound;
    [SerializeField] private UIButtonProvider _btnGoToLanguage;
    [SerializeField] private UIButtonProvider _btnGoToBack;

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
        _btnGoToVideo.AddListener(OnGoToVideoButton);
        _btnGoToSound.AddListener(OnGoToSoundButton);
        _btnGoToLanguage.AddListener(OnGoToLanguageButton);
        _btnGoToBack.AddListener(OnGoToBackButton);
    }

    private void RemoveListener()
    {
        _btnGoToVideo.RemoveListener(OnGoToVideoButton);
        _btnGoToSound.RemoveListener(OnGoToSoundButton);
        _btnGoToLanguage.RemoveListener(OnGoToLanguageButton);
        _btnGoToBack.RemoveListener(OnGoToBackButton);
    }

    public override void Close()
    {
        RemoveListener();
        UIAnimationComponent
        .UsingAnimation(gameObject)
        .ApplyPresetClose(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayCloseAnimation();
        base.Close();
    }

    private void OnGoToBackButton() => UIRootManager.OpenScreen<UIMainScreen>();
    private void OnGoToLanguageButton() => UIRootManager.OpenScreen<UISettingLanguageScreen>();
    private void OnGoToSoundButton() => UIRootManager.OpenScreen<UISettingSoundScreen>();
    private void OnGoToVideoButton() => UIRootManager.OpenScreen<UISettingVideoScreen>();
}
