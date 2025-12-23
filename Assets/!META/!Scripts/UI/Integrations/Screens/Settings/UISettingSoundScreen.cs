using System;
using UnityEngine;

public class UISettingSoundScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToBack;
    [SerializeField] private UISliderProvider _slSoundMaster;
    [SerializeField] private UISliderProvider _slSoundMusic;


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
        _slSoundMaster.AddListener(OnSoundMasterChanged);
        _slSoundMusic.AddListener(OnSoundMusicChanged);
    }

    private void OnSoundMusicChanged(float value)
    {
        SaveService.Save("SoundMusic", value);
    }

    private void OnSoundMasterChanged(float value)
    {
        SaveService.Save("SoundMaster", value);
    }

    private void RemoveListener()
    {
        _btnGoToBack.RemoveListener(OnGoToBackButton);
        _slSoundMaster.RemoveListener(OnSoundMasterChanged);
        _slSoundMusic.RemoveListener(OnSoundMusicChanged);
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
