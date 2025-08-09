using Mirror;
using VContainer;
using VContainer.Unity;

public abstract class EntryPointScene : LifetimeScope
{
    protected override void Awake()
    {
        base.Awake();
        parentReference = ParentReference.Create<EntryPointProject>();
        var networkManager = Parent.Container.Resolve<NetworkManager>();
        if (networkManager.mode == NetworkManagerMode.ServerOnly)
        {
            Destroy(gameObject);
            return;
        }
    }
}
