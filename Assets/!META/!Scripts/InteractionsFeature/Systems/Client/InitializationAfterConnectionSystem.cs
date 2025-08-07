using BitterECS.Core;
using Mirror;

public class InitializationAfterConnectionSystem : IClientConnected, IClientDisconnected, IServerConnected
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public async void Connect()
    {
        await SceneLoader.LoadSceneAsync(SceneTypes.Menu);
    }

    public void Connect(NetworkConnectionToClient client)
    {
        
    }

    public async void Disconnect()
    {
        await SceneLoader.LoadSceneAsync(SceneTypes.Menu);
    }
}
