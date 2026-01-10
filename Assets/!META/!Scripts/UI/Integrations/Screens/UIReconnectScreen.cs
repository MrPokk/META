using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;

public class UIReconnectScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToReconnect;
    [SerializeField] private UIButtonProvider _btnGoToExit;

    private CancellationTokenSource _connectionCts;
    private EntryPointClient _entryPointClient;
    private bool _isConnecting;

    [Inject]
    private void Construct(EntryPointClient entryPointClient)
    {
        _entryPointClient = entryPointClient;
    }

    public override void Open()
    {
        base.Open();

        AddListeners();
        SetupNavigation();
    }

    public override void Close()
    {
        CancelConnectionAttempt();
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

    private void OnReconnectButtonClicked()
    {
        if (_isConnecting)
            return;

        StartReconnection().Forget();
    }

    private async UniTaskVoid StartReconnection()
    {
        _isConnecting = true;

        try
        {
            await AttemptReconnection();
        }
        finally
        {
            _isConnecting = false;
        }
    }

    private async UniTask AttemptReconnection()
    {
        CancelConnectionAttempt();

        _connectionCts = new CancellationTokenSource();
        var token = _connectionCts.Token;

        _entryPointClient.SetupConnection();

        const int MaxAttempts = 5;
        const int DelayMs = 2000;

        var isConnected = await TryConnectWithRetries(MaxAttempts, DelayMs, token);

        if (isConnected)
        {
            await HandleSuccessfulConnection(token);
        }
        else
        {
            HandleFailedConnection();
        }
    }

    private async UniTask<bool> TryConnectWithRetries(int maxAttempts, int delayMs, CancellationToken token)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (token.IsCancellationRequested)
                return false;

            if (NetworkUtility.IsClientActive())
                return true;

            if (attempt < maxAttempts - 1)
            {
                await UniTask.Delay(delayMs, cancellationToken: token);
            }
        }

        return false;
    }

    private async UniTask HandleSuccessfulConnection(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        await SceneNetworkProvider.ChangeScene(SceneTypes.StartFloor);
        Close();
    }

    private void HandleFailedConnection()
    {
        Debug.LogWarning("Failed to connect after multiple attempts");
    }

    private void CancelConnectionAttempt()
    {
        _connectionCts?.Cancel();
        _connectionCts?.Dispose();
        _connectionCts = null;
    }

    private void OnDestroy()
    {
        CancelConnectionAttempt();
    }
}
