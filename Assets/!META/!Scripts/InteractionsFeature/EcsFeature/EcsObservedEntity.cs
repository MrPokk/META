using BitterECS.Core;

public class EcsObservedEntity : EcsEntity
{
    public override void Registration()
    {
        Add(new ViewComponent());
        Add(new TransformComponent());
        Add(new NetworkSyncComponent());
    }
}
