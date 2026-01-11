using BitterECS.Core;

public class ReconnectSystem :  IClientDisconnected, IClientError
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public void Disconnect()
    {
        UIRootManager.CloseScreen();
        UIRootManager.CloseAllPopups();
        UIRootManager.OpenScreen<UIReconnectScreen>();
    }

    public void OnError()
    {
        UIRootManager.CloseScreen();
        UIRootManager.CloseAllPopups();
        UIRootManager.OpenScreen<UIReconnectScreen>();
    }
}
