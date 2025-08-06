using BitterECS.Core;

public class SetupSceneSystem : IClientConnected, IClientDisconnected
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    public async void Connect()
    {
        await SceneLoader.LoadSceneAsync(SceneTypes.LocalGame);
    }

    public async void Disconnect()
    {
        await SceneLoader.LoadSceneAsync(SceneTypes.Menu);
    }
}
