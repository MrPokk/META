using UnityEngine;
using VContainer;

public class UIMainScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToGameplay;
    [SerializeField] private UIButtonProvider _btnGoToSettings;
    [SerializeField] private UIButtonProvider _btnGoToExit;

    public override async void Open()
    {
        AddListener();
        UIAnimationComponent
        .UsingAnimation(gameObject)
        .ApplyPresetOpen(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayOpenAnimation();
        base.Open();
    }

    public override async void Close()
    {
        RemoveListener();
        UIAnimationComponent
        .UsingAnimation(gameObject)
        .ApplyPresetClose(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayCloseAnimation(() => base.Close());
    }

    private void AddListener()
    {
        _btnGoToGameplay.AddListener(OnGoToGameplayButted);
        _btnGoToSettings.AddListener(OnGoToSettingsButted);
        _btnGoToExit.AddListener(OnGoToExitButted);
    }

    private void RemoveListener()
    {
        _btnGoToGameplay.RemoveListener(OnGoToGameplayButted);
        _btnGoToSettings.RemoveListener(OnGoToSettingsButted);
        _btnGoToExit.RemoveListener(OnGoToExitButted);
    }

    private void OnGoToExitButted()
    {
        Application.Quit();
    }

    private void OnGoToSettingsButted()
    {
        UIRootManager.OpenScreen<UISettingScreen>();
    }

    private void OnGoToGameplayButted()
    {
        Container.Resolve<EntryPointClient>().SetupConnection();
        SceneNetworkProvider.ChangeScene(SceneTypes.StartRoom);
        Close();
    }
}

