using Mirror;
using VContainer;
using VContainer.Unity;

public abstract class EntryPointScene : LifetimeScope
{
    protected override void Awake()
    {
        parentReference = ParentReference.Create<EntryPointProject>();
        base.Awake();
    }

    private void Start()
    {
        var networkManager = Parent.Container.Resolve<NetworkManager>();
        if (networkManager.mode == NetworkManagerMode.ServerOnly)
        {
            Destroy(gameObject);
            return;
        }
        Bootstrap();
    }

    protected abstract void Bootstrap();
}
