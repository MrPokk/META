
using BitterECS.Core;
using Mirror;

public class ServerHandler : IServerConnected, IServerDisconnected
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public void Connect(NetworkConnectionToClient client)
    {
        SceneNetworkProvider.Instance.InitializeClientScene(client);
    }

    public void Disconnect(NetworkConnectionToClient client)
    {
         SceneNetworkProvider.Instance.RemoveClientScene(client);
         
    }
}
