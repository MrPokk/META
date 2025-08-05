using BitterECS.Core;

public class PlayerEntity : EcsEntity
{
    public override void Registration()
    {
        Add(new ViewComponent());
        Add(new ControllableComponent());
        Add(new MoveComponent(5f));
        Add(new NetworkSyncComponent());
    }
}
