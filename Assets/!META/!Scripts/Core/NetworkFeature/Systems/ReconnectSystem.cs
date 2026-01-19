using BitterECS.Core;

public class ReconnectSystem : IClientDisconnected, IClientError
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public void Disconnect()
    {
        UIRootManager.OpenScreen<UIReconnectScreen>();
        UIRootManager.CloseAllPopups();
    }

    public void OnError()
    {
        UIRootManager.OpenScreen<UIReconnectScreen>();
        UIRootManager.CloseAllPopups();
    }
}
