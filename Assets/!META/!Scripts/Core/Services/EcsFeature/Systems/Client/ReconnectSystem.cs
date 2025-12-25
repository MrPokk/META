using BitterECS.Core;

public class ReconnectSystem : IClientConnected, IClientDisconnected, IClientError
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public void Connect()
    {
        
    }

    public void Disconnect()
    {
        UIRootManager.OpenScreen<UIReconnectScreen>();
    }

    public void OnError()
    {
        UIRootManager.OpenScreen<UIReconnectScreen>();
    }
}
