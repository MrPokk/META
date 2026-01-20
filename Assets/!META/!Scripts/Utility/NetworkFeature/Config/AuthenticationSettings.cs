using System;
using Mirror;
using UnityEngine;
#region Data Structures

[Serializable]
public struct AuthenticationSettings
{
    [Header("<size=16>About Settings</size>")]
    [Header("Authentication")]
    public NetworkAuthenticator authenticator;
}

#endregion
