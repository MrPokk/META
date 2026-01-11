using BitterECS.Core;

public class PlayerAnimationSystem : IClientConnectedFixedRun
{
    public Priority PrioritySystem => Priority.Medium;

    private EcsFilter _ecsFilter =
    Build.For<PlayerPresenter>()
         .Filter()
         .Include<StateComponent>();


    public void FixedRun()
    {
        foreach (var player in _ecsFilter)
        {
            if (player.Provider is not PlayerProvider monoProvider)
            {
                continue;
            }

            if (monoProvider == null || monoProvider.gameObject == null)
            {
                continue;
            }

            ref var state = ref player.Get<StateComponent>();

            monoProvider.PlayerModelComponent.SetSpeedAnimation();
            monoProvider.PlayerModelComponent.SetAnimationState(state.state);
        }
    }
}
