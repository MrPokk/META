using System;
using UnityEngine;

public class UISettingControllableScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToBack;
    [SerializeField] private UISliderProvider _slSensitivity;


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
        _btnGoToBack.AddListener(OnGoToBackButton);
        _slSensitivity.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        SaveService.Save("Sensitivity", value);
    }

    private void RemoveListener()
    {
        _btnGoToBack.RemoveListener(OnGoToBackButton);
        _slSensitivity.RemoveListener(OnSensitivityChanged);
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
}
