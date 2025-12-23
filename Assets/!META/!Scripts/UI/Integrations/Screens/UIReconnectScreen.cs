using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class UIReconnectScreen : UIScreen
{
    [SerializeField] private UIButtonProvider _btnGoToReconnect;
    [SerializeField] private UIButtonProvider _btnGoToExit;
    
    private CancellationTokenSource _connectionCts;

    public override void Open()
    {
        AddListener();

        UINavigationComponent
            .UsingNavigation(gameObject)
            .ApplyFirstSelected()
            .ApplyNavigation(
                _btnGoToReconnect,
                _btnGoToExit);

        base.Open();
    }

    public override void Close()
    {
        _connectionCts?.Cancel();
        _connectionCts?.Dispose();
        _connectionCts = null;
        
        RemoveListener();
        base.Close();
    }

    private void AddListener()
    {
        _btnGoToReconnect.AddListener(OnGoToGameplayButtonClicked);
        _btnGoToExit.AddListener(OnGoToExitButtonClicked);
    }

    private void RemoveListener()
    {
        _btnGoToReconnect.RemoveListener(OnGoToGameplayButtonClicked);
        _btnGoToExit.RemoveListener(OnGoToExitButtonClicked);
    }

    private void OnGoToExitButtonClicked()
    {
        UIRootManager.OpenScreen<UIMainScreen>();
    }

    private async void OnGoToGameplayButtonClicked()
    {
        _connectionCts?.Cancel();
        _connectionCts?.Dispose();
        
        _connectionCts = new CancellationTokenSource();
        var token = _connectionCts.Token;
        
        Container.Resolve<EntryPointClient>().SetupConnection();
        
        var isConnected = await TryConnectWithRetries(5, 2000, token);
        
        if (isConnected)
        {
            SceneNetworkProvider.ChangeScene(SceneTypes.StartFloor);
            Close();
        }
        else
        {
            Debug.LogWarning("Failed to connect after 5 attempts");
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
                try
                {
                    await UniTask.Delay(delayMs, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
            }
        }
        
        return false;
    }
}
