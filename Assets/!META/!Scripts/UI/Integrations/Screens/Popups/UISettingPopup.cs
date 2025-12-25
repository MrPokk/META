using System;
using BitterECS.Core;
using UnityEngine;

public class UISettingPopup : UIPopup
{
    [SerializeField] private UISliderProvider _slSensitivity;
    [SerializeField] private UISliderProvider _slSoundMaster;
    [SerializeField] private UISliderProvider _slSoundMusic;

    private EcsFilter _ecsEntities =
        Build.For<PlayerPresenter>()
        .Filter()
        .Include<ControllableComponent>();

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
    }

    private void OnSensitivityChanged(float value)
    {
        foreach (var entity in _ecsEntities)
        {
            var playerProvider = entity.Provider as PlayerProvider;
            playerProvider.CameraObjectComponent.SetMultipleAxisController();
        }
    }
}
