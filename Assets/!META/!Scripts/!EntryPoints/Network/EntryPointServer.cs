using kcp2k;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointServer : IStartable
{
    private readonly NetworkServerConfig _networkConfig;
    private readonly NetworkManager _networkManager;

    [Inject]
    public EntryPointServer(NetworkServerConfig networkConfig, NetworkManager networkManager)
    {
        _networkConfig = networkConfig;
        _networkManager = networkManager;
    }

    public void Start()
    {
        InitializeServer();
    }

    private void InitializeServer()
    {
        _networkConfig.ConfigureServer(_networkManager);
        SetupTransports();

        _networkManager.StartServer();
        Debug.Log("[Network] Server started!");
    }

    private void SetupTransports()
    {
#if UNITY_WEBGL
        _networkManager.transport = _networkManager.GetComponent<SimpleWebTransport>();
        Debug.Log("[Network] Using WebSockets for WebGL");
#else
        _networkManager.transport = _networkManager.GetComponent<KcpTransport>();
        Debug.Log("[Network] Using KCP for Desktop/Server");
#endif
    }
}
