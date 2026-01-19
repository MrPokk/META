using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

public class UIReconnectScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToReconnect;
    [SerializeField] private UIButtonProvider _btnGoToExit;

    public override void Open()
    {
        base.Open();
        AddListeners();
        SetupNavigation();
    }

    public override void Close()
    {
        RemoveListeners();
        base.Close();
    }

    private void SetupNavigation()
    {
        UINavigationComponent
            .UsingNavigation(gameObject)
            .ApplyFirstSelected()
            .ApplyNavigation(_btnGoToReconnect, _btnGoToExit);
    }

    private void AddListeners()
    {
        _btnGoToReconnect.AddListener(OnReconnectButtonClicked);
        _btnGoToExit.AddListener(OnExitButtonClicked);
    }

    private void RemoveListeners()
    {
        _btnGoToReconnect.RemoveListener(OnReconnectButtonClicked);
        _btnGoToExit.RemoveListener(OnExitButtonClicked);
    }

    private void OnExitButtonClicked()
    {
        UIRootManager.OpenScreen<UIMainScreen>();
    }

    private async void OnReconnectButtonClicked()
    {
        if (NetworkUtility.ReconnectService.IsConnecting)
            return;

        var isConnected = await NetworkUtility.ReconnectService.ReconnectAsync();

        if (isConnected)
            await HandleSuccessfulConnection();
        else
            HandleFailedConnection();
    }

    private async UniTask HandleSuccessfulConnection()
    {
        await SceneNetworkProvider.ChangeScene(SceneTypes.StartFloor);
        Close();
    }

    private void HandleFailedConnection()
    {
        Debug.LogWarning("Failed to connect after multiple attempts");
    }

    private void OnDestroy()
    {
        NetworkUtility.ReconnectService?.Dispose();
    }
}
