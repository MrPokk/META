using System;
using System.Collections;
using Mirror;
using UnityEngine;

public static class NetworkUtility
{

    public static void SendMessage<T>(T value, NetworkConnection target = null) where T : struct, NetworkMessage
    {
        if (NetworkServer.active && target != null)
        {
            target.Send<T>(value);
        }
        else if (NetworkServer.active)
        {
            NetworkServer.SendToAll<T>(value);
        }
        else if (NetworkClient.active)
        {
            CoroutineUtility.Run(WaitingToSend<T>(value));
        }
    }

    private static IEnumerator WaitingToSend<T>(T message) where T : struct, NetworkMessage
    {
        yield return new WaitUntil(() => NetworkClient.connection.isReady);
        NetworkClient.Send<T>(message);
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
