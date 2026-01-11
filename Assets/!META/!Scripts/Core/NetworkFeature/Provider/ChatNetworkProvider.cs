using BitterECS.Core;
using Cysharp.Threading.Tasks;
using Mirror;

public class ChatNetworkProvider : IProviderHandler
{
    public static void SendChatMessage(string message) =>
    NetworkUtility.SendMessage<ChatMessage>(new(NetworkUtility.ClientID, message)).Forget();

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
            ValidateMessage(ref message);
            AddMessageToPlayerName(ref message);
            NetworkUtility.SendMessage(message, clientOnScene).Forget();
        }
    }

    private void AddMessageToPlayerName(ref ChatMessage message)
    {
        message.sender = $"Guest{message.ownerId}";
    }

    private void ValidateMessage(ref ChatMessage message)
    {
        //TODO: VALIDATE MESSAGE
    }
}
