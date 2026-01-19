using Cysharp.Threading.Tasks;
using Mirror;
using VContainer;

public class NetworkUtility
{
    public static NetworkType Type { get; private set; }
    public static NetworkReconnectService ReconnectService { get; private set; }
    public static NetworkMessagingService MessagingService { get; private set; }

    public NetworkUtility(NetworkConfig networkConfig)
    {
        ReconnectService = new NetworkReconnectService();
        MessagingService = new NetworkMessagingService();
        Type = networkConfig.NetworkType;

#if !UNITY_EDITOR
        InitializeServerConfig(networkConfig);
#endif
    }

#if !UNITY_EDITOR
    private void InitializeServerConfig(NetworkConfig config)
    {
        if (config.NetworkType == NetworkType.Server)
        {
            config.LoadOrSaveServerConfig();
        }
    }
#endif

    public static bool IsLocalPlayer(uint netId)
    {
        return Type == NetworkType.Client && IsClientReady() && netId == EntryPointClient.ClientID;
    }

    public static async UniTask SendMessage<T>(T message, NetworkConnection targetConnection = null) where T : struct, NetworkMessage
    {
        var service = MessagingService ?? new NetworkMessagingService();
        await service.SendMessage(message, targetConnection);
    }

    public static bool IsClientReady()
    {
        return NetworkClient.connection != null &&
               NetworkClient.ready &&
               NetworkClient.active;
    }

    public static bool IsServerActive() => NetworkServer.active;

}
