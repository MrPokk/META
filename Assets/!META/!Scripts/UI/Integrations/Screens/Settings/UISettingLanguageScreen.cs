using System;
using Gley.Localization;
using UnityEngine;

public class UISettingLanguageScreen : UIScreen
{
    [SerializeField] private UISelectorProvider _slLanguage;
    [SerializeField] private UIButtonProvider _btnGoToBack;

    public override void Open()
    {
        AddListener();
        UIAnimationComponent.UsingAnimation(gameObject)
        .ApplyPreset(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayOpenAnimation();
        base.Open();
    }

    private void AddListener()
    {
        _slLanguage.AddListener(OnLanguageChanged);
        _btnGoToBack.AddListener(OnGoToBackButton);
    }

    private void RemoveListener()
    {
        _slLanguage.RemoveListener(OnLanguageChanged);
        _btnGoToBack.RemoveListener(OnGoToBackButton);
    }

    public override void Close()
    {
        RemoveListener();
        UIAnimationComponent.UsingAnimation(gameObject)
        .ApplyPreset(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayCloseAnimation();
        base.Close();
    }

    private void OnGoToBackButton() => UIRootManager.OpenScreen<UISettingScreen>();

    private void OnLanguageChanged(int arg0) => UIUpdateLocalized.SetLanguage(_slLanguage.GetSelectedLanguage());
}
