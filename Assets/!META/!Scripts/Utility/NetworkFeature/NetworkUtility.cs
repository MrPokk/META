using System;
using Mirror;
using Cysharp.Threading.Tasks;

public static class NetworkUtility
{
    public static void SendMessage<T>(T value, NetworkConnection target = null) where T : struct, NetworkMessage
    {
        if (NetworkServer.active && target != null)
        {
            target.Send(value);
        }
        else if (NetworkServer.active)
        {
            NetworkServer.SendToAll(value);
        }
        else if (NetworkClient.active)
        {
            WaitingToSend(value).Forget();
        }
        else
        {
            LoggerUtility.Warning("Waiting for connection...");
        }
    }

    private static async UniTaskVoid WaitingToSend<T>(T message) where T : struct, NetworkMessage
    {
        try
        {
            await UniTask.WaitUntil(() =>
                NetworkClient.connection != null &&
                NetworkClient.connection.isReady
            );

            NetworkClient.Send(message);
        }
        catch (OperationCanceledException)
        {
            LoggerUtility.Warning("NetworkClient is not ready");
        }
        catch (Exception ex)
        {
            LoggerUtility.Critical($"Failed to send network message: {ex.Message}");
        }
    }

    public static bool IsClientActive()
    {
        if (!NetworkClient.active)
        {
            LoggerUtility.Error("NetworkClient is not active");
            return false;
        }
        return true;
    }

    public static bool IsServerActive()
    {
        if (!NetworkServer.active)
        {
            LoggerUtility.Error("NetworkServer is not active");
            return false;
        }
        return true;
    }
}
