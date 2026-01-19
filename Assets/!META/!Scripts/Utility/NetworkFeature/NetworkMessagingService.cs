using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using UnityEngine;

public class NetworkMessagingService
{
    private readonly Stack<Type> _messages = new();

    public async UniTask SendMessage<T>(T message, NetworkConnection targetConnection = null) where T : struct, NetworkMessage
    {
        if (NetworkServer.active)
        {
            SendAsServer(message, targetConnection);
            return;
        }

        if (NetworkClient.active)
        {
            await SendAsClientAsync(message);
            return;
        }

        LoggerUtility.Warning("[NetworkMessagingService] Cannot send message: No active Server or Client.");
    }

    private void SendAsServer<T>(T message, NetworkConnection targetConnection) where T : struct, NetworkMessage
    {
        if (targetConnection != null)
        {
            targetConnection.Send(message);
            return;
        }

        NetworkServer.SendToAll(message);
    }

    private async UniTask SendAsClientAsync<T>(T message) where T : struct, NetworkMessage
    {
        try
        {
            if (_messages.Any() && _messages.Peek() == typeof(T))
            {
                return;
            }

            if (!IsConnectionReady())
            {
                await UniTask.WaitUntil(IsConnectionReady);
            }

            _messages.Push(typeof(T));

            NetworkClient.Send(message);

            _messages.Pop();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            LoggerUtility.Critical($"[NetworkMessagingService] Failed to send client message {typeof(T).Name}: {ex.Message}");
        }
    }

    private bool IsConnectionReady() => NetworkClient.connection != null && NetworkClient.connection.isReady;
}
