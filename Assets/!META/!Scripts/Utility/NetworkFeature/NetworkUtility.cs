using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;

public class NetworkUtility
{
    public static IEnumerator WaitingToConnect(NetworkConnection target, Action callback)
    {
        if (target == null) { LoggerUtility.Error("WaitingClientToConnect: target is null"); yield break; }

        yield return new WaitUntil(() => target.isReady);
        callback?.Invoke();
    }

    public static void SetupHandlers(IEnumerable<IProviderHandler> handlers)
    {
        foreach (var handler in handlers)
        {
            if (NetworkManager.singleton.mode == NetworkManagerMode.ServerOnly)
                handler.HandlersServer();
            else if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
                handler.HandlersClient();
            else
                throw new Exception($"Invalid network mode: {NetworkManager.singleton.mode}");
        }
    }

    public static Guid GetStableGuid(Type type)
    {
        string stableName = type.AssemblyQualifiedName;
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(stableName));
            return new Guid(hash);
        }
    }

    public static bool IsClientActive()
    {
        if (!NetworkClient.active)
        {
            LoggerUtility.Error("NetworkClient is not active");
            return false;
        }
        return NetworkClient.active;
    }

    public static bool IsServerActive()
    {
        if (!NetworkServer.active)
        {
            LoggerUtility.Error("NetworkServer is not active");
            return false;
        }
        return NetworkServer.active;
    }
}
