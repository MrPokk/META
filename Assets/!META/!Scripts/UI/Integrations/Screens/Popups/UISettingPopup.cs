using System;
using BitterECS.Core;
using UnityEngine;
using static BitterECS.Core.EcsFilter;

public class UISettingPopup : UIPopup
{
    [SerializeField] private UISliderProvider _slSensitivity;
    [SerializeField] private UISliderProvider _slSoundMaster;
    [SerializeField] private UISliderProvider _slSoundMusic;
    [SerializeField] private UISwitchProvider _swShowPlayers;

    private FilterEnumerator _ecsSensitivity =>
        Build.For<PlayerPresenter>()
        .Filter()
        .Include<ControllableComponent>()
        .Collect();

    private FilterEnumerator _ecsToggle =>
        Build.For<PlayerPresenter>()
        .Filter()
        .Exclude<ControllableComponent>()
        .Collect();

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
        _slSensitivity.AddListener(OnSensitivityChanged);
        _swShowPlayers.AddListener(OnShowPlayerChanged);
    }

    private void OnShowPlayerChanged(bool value)
    {
        foreach (var entity in _ecsToggle)
        {
            var playerProvider = entity.Provider as PlayerProvider;
            playerProvider.PlayerModelComponent.SetView(value);
        }
    }

    private void OnSensitivityChanged(float value)
    {
        foreach (var entity in _ecsSensitivity)
        {
            var playerProvider = entity.Provider as PlayerProvider;
            playerProvider.CameraObjectComponent.SetMultipleAxisController();
        }
    }
}
