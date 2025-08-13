using VContainer;

public class MenuEntryPoint : EntryPointScene
{
    public void ConnectGame()
    {//TODO Перенести в UI Root
        var entryPointClient = Parent.Container.Resolve<EntryPointClient>();
        entryPointClient.SetupConnection();

        SceneNetworkProvider.SendRequest(SceneTypes.StartRoom);
    }

    protected override void Bootstrap()
    {
        
    }
}
