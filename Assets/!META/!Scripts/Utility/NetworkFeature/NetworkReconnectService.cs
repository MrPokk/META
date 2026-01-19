using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NetworkReconnectService : IDisposable
{
    private CancellationTokenSource _connectionCts;
    public bool IsConnecting { get; private set; }

    public async UniTask<bool> ReconnectAsync(int maxAttempts = 5, int delayMs = 2000, CancellationToken externalToken = default)
    {
        if (IsConnecting)
        {
            LoggerUtility.Warning("Reconnection already in progress.");
            return false;
        }

        IsConnecting = true;
        CancelConnectionAttempt();
        _connectionCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

        try
        {
            LoggerUtility.Info("Starting reconnection sequence...");
            EntryPointClient.SetupConnection();
            return await TryConnectWithRetries(maxAttempts, delayMs, _connectionCts.Token);
        }
        catch (OperationCanceledException)
        {
            LoggerUtility.Info("Reconnection canceled.");
            return false;
        }
        catch (Exception ex)
        {
            LoggerUtility.Critical($"Reconnection critical error: {ex}");
            return false;
        }
        finally
        {
            IsConnecting = false;
            DisposeCts();
        }
    }

    private async UniTask<bool> TryConnectWithRetries(int maxAttempts, int delayMs, CancellationToken token)
    {
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            token.ThrowIfCancellationRequested();

            if (NetworkUtility.IsClientReady())
            {
                LoggerUtility.Info("Reconnection successful!");
                return true;
            }

            LoggerUtility.Info($"Reconnection attempt {attempt}/{maxAttempts}...");

            if (attempt < maxAttempts)
            {
                await UniTask.Delay(delayMs, cancellationToken: token);
            }
        }

        LoggerUtility.Warning("Reconnection failed after all attempts.");
        return false;
    }

    public void CancelConnectionAttempt()
    {
        _connectionCts?.Cancel();
        DisposeCts();
    }

    private void DisposeCts()
    {
        _connectionCts?.Dispose();
        _connectionCts = null;
    }

    public void Dispose()
    {
        CancelConnectionAttempt();
    }
}
