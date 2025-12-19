using BitterECS.Core;

public class PlayerAnimationSystem : IClientConnectedFixedRun
{
    public Priority PrioritySystem => Priority.Medium;

    private EcsFilter _ecsFilter = 
    Build.For<PlayerPresenter>()
         .Filter()
         .Include<StateComponent>();

    private string _isWalk = "IsWalk";
    private string _isIdle = "IsIdle";

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

            SetSpeedAnimation(monoProvider);
            switch (state.state)
            {
                case StateComponent.State.Idle:
                    monoProvider.animator.SetTrigger(_isIdle);
                    break;
                case StateComponent.State.Moving:
                    monoProvider.animator.SetTrigger(_isWalk);
                    break;
                default:
                    break;
            }
        }
    }

    private static void SetSpeedAnimation(PlayerProvider monoProvider)
    {
        if (monoProvider.CharacterController == null)
            return;

        if (monoProvider.animator == null)
            return;

        var animationSpeedMultiplier = 1;
        var speedPlayer = monoProvider.CharacterController.velocity.magnitude;
        monoProvider.animator.speed = speedPlayer > 0.1f ? speedPlayer * animationSpeedMultiplier : 1;
    }
}
