using System;
using UnityEngine;
using VContainer;

public class UIReconnectScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToReconnect;

    [SerializeField] private UIButtonProvider _btnGoToExit;
    private void OnEnable()
    {
        _btnGoToReconnect.onClick.AddListener(OnGoToGameplayButtonClicked);
        _btnGoToExit.onClick.AddListener(OnGoToExitButtonClicked);
    }
    private void OnDisable()
    {
        _btnGoToReconnect.onClick.RemoveListener(OnGoToGameplayButtonClicked);
        _btnGoToExit.onClick.RemoveListener(OnGoToExitButtonClicked);
    }

    private void OnGoToExitButtonClicked()
    {
        Application.Quit();
    }

    private void OnGoToGameplayButtonClicked()
    {
        Container.Resolve<EntryPointClient>().SetupConnection();
    }
}
