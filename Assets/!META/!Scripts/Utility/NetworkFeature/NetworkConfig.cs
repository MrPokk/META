using UnityEngine;
using Mirror;
using System;

[CreateAssetMenu(fileName = "NetworkServerConfig", menuName = "Network/Global Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("<size=18>Network Settings</size>")]
    [SerializeField] internal NetworkManager _networkManager;

    [SerializeField] private CommonSettings _common = CommonSettings.Default;
    [SerializeField] private AuthenticationSettings _auth;

    [Header("<size=16>Transport Settings</size>")]
    [SerializeField] private KcpTransportConfig _kcpConfig = KcpTransportConfig.Default;
    [SerializeField] private WebSocketTransportConfig _webSocketConfig = WebSocketTransportConfig.Default;

    [Header("Logging Settings")]
    [SerializeField] private bool _exceptionsDisconnect = true;

    public NetworkManager NetworkPrefab => _networkManager;
    public KcpTransportConfig KcpConfig => _kcpConfig;
    public WebSocketTransportConfig WebSocketConfig => _webSocketConfig;
    public NetworkAuthenticator Authenticator => _auth.authenticator;
    public bool ExceptionsDisconnect => _exceptionsDisconnect;

    public string NetworkAddress => _common.networkAddress;
    public int MaxConnections => _common.maxConnections;
    public CommonSettings Common => _common;

    public NetworkType NetworkType
    {
        get
        {
#if (DEDICATED_SERVER || SERVER || UNITY_SERVER || MIRROR_SERVER) && !UNITY_EDITOR
            return NetworkType.Server;
#else
            return _common.networkType;
#endif
        }
    }

    private void OnValidate()
    {
        if (_networkManager != null)
        {
            NetworkConfigLogic.ApplyToManager(this, _networkManager);
        }
    }
    public void LoadToApply(NetworkManager manager)
    {
        NetworkConfigPersistence.LoadOrSaveServerConfig(this);
        NetworkConfigLogic.ApplyToManager(this, manager);
    }

    public void Apply(NetworkManager manager)
    {
        NetworkConfigLogic.ApplyToManager(this, manager);
    }

    public void UpdateSettings(NetworkConfigData data)
    {
        if (data == null) return;

        if (data.commonSettings.HasValue) _common = data.commonSettings.Value;
        if (data.kcpConfig.HasValue) _kcpConfig = data.kcpConfig.Value;
        if (data.webSocketConfig.HasValue) _webSocketConfig = data.webSocketConfig.Value;
    }
}
