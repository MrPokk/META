using BitterECS.Core;
using Cysharp.Threading.Tasks;
using Mirror;

public class MessageNetworkProvider : IProviderHandler
{
    public static async UniTask SendChatMessage(string message)
    => await NetworkUtility.SendMessage<ChatMessage>(new(message));

    public void HandlersClient()
    {
        NetworkClient.RegisterHandler<ChatMessage>(OnClientChatMessage);
    }

    private void OnClientChatMessage(ChatMessage message)
    {
        EcsSystems.Run<IClientChatMessage>(system => system.OnMessage(message));
    }

    public void HandlersServer()
    {
        NetworkServer.RegisterHandler<ChatMessage>(OnServerChatMessage);
    }

    private void OnServerChatMessage(NetworkConnectionToClient client, ChatMessage message)
    {
        var isContainScene = ConnectionInfo.ClientToScene.TryGetValue(client, out var sceneTypes);
        if (!isContainScene)
        {
            return;
        }

        foreach (var clientOnScene in ConnectionInfo.SceneToConnections[sceneTypes])
        {
            NetworkUtility.SendMessage(message, clientOnScene).Forget();
        }
    }
}
