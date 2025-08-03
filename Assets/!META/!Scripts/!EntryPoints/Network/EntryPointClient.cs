using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EntryPointClient : IStartable
{

    private readonly NetworkClientConfig _clientConfig;
    private readonly NetworkManager _networkManager;

    [Inject]
    public EntryPointClient(NetworkClientConfig clientConfig, NetworkManager networkManager)
    {
        _clientConfig = clientConfig;
        _networkManager = networkManager;
    }

    public async void Start()
    {
#if UNITY_WEBGL
        _networkManager.networkAddress = _clientConfig.webSocketServerUrl;
#else
        _networkManager.networkAddress = _clientConfig.serverIP;
#endif

        _networkManager.StartClient();
        Debug.Log("[Network] Client started!");

        await SceneLoader.LoadSceneAsync(SceneTypes.Menu);
    }
}
