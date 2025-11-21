using System;
using UnityEngine;
using VContainer;

public class UIMainScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToGameplay;
    [SerializeField] private UIButtonProvider _btnGoToSettings;
    [SerializeField] private UIButtonProvider _btnGoToExit;

    private void OnEnable()
    {
        _btnGoToGameplay.onClick.AddListener(OnGoToGameplayButtonClicked);
        _btnGoToSettings.onClick.AddListener(OnGoToSettingsButtonClicked);
        _btnGoToExit.onClick.AddListener(OnGoToExitButtonClicked);
    }
    private void OnDisable()
    {
        _btnGoToGameplay.onClick.RemoveListener(OnGoToGameplayButtonClicked);
        _btnGoToSettings.onClick.RemoveListener(OnGoToSettingsButtonClicked);
        _btnGoToExit.onClick.RemoveListener(OnGoToExitButtonClicked);
    }

    private void OnGoToExitButtonClicked()
    {
        Application.Quit();
    }

    private void OnGoToSettingsButtonClicked()
    {
        UIRootManager.OpenScreen<UISettingScreen>();
    }

    private void OnGoToGameplayButtonClicked()
    {
        Container.Resolve<EntryPointClient>().SetupConnection();
        SceneNetworkProvider.ChangeScene(SceneTypes.StartRoom);
        Close();
    }
}
