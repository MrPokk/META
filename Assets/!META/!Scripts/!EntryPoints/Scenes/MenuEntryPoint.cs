using VContainer;
using VContainer.Unity;

public class MenuEntryPoint : LifetimeScope
{
    public void ConnectGame()
    {
        var entryPointClient = Parent.Container.Resolve<EntryPointClient>();
        entryPointClient.SetupConnection();
    }
}
