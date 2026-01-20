using System;
using UnityEngine;
#region Data Structures

[Serializable]
public struct CommonSettings
{
    [Header("<size=16>Common Settings</size>")]
    public string networkAddress;
    public int maxConnections;
    public NetworkType networkType;

    public static CommonSettings Default => new()
    {
        networkAddress = "localhost",
        maxConnections = 100,
        networkType = NetworkType.None
    };
}

#endregion
