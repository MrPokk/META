using BitterECS.Core;
using UnityEngine;
using static PlayerProvider;

public partial class PlayableMoveSystem : IClientConnectedFixedRun
{
    public Priority PrioritySystem => Priority.High;

    public void FixedRun()
    {
        var query = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Collect();

        foreach (var entity in query)
        {
            ref var movingComponent = ref entity.Get<MovingComponent>();
            ref var controllableComponent = ref entity.Get<ControllableComponent>();
            ref var stateComponent = ref entity.Get<StateComponent>();

            if (entity.Provider is PlayerProvider monoProvider)
            {
                if (monoProvider == null || monoProvider.gameObject == null)
                    continue;

                if (monoProvider.CharacterController == null)
                    continue;

                GetPlayerDirection(monoProvider, out var playerForward, out var playerRight);

                var directionTo = (playerForward * controllableComponent.input.y +
                                  playerRight * controllableComponent.input.x).normalized;

                monoProvider.CharacterController.SimpleMove(directionTo * movingComponent.speed);
                if (directionTo != Vector3.zero)
                {
                    stateComponent.state = StateComponent.State.Moving;
                }
                else
                {
                    stateComponent.state = StateComponent.State.Idle;
                }
            }
        }
    }

    private static void GetPlayerDirection(PlayerProvider monoProvider, out Vector3 playerForward, out Vector3 playerRight)
    {
        playerForward = monoProvider.transform.forward;
        playerForward.y = 0;
        playerForward.Normalize();

        playerRight = monoProvider.transform.right;
        playerRight.y = 0;
        playerRight.Normalize();
    }
}
