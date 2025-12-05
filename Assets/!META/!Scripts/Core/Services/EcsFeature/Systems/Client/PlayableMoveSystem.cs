using BitterECS.Core;
using UnityEngine;

public class PlayableMoveSystem : IClientConnectedFixedRun
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

            if (entity.Provider is PlayerProvider monoProvider)
            {
                if (monoProvider.CharacterController != null)
                {
                    GetPlayerDirection(monoProvider, out var playerForward, out var playerRight);

                    var directionTo = (playerForward * controllableComponent.input.y +
                                      playerRight * controllableComponent.input.x).normalized;

                    monoProvider.CharacterController.SimpleMove(directionTo * movingComponent.speed);

                    EcsSystems.Run<IPlayerUsingSystem>(system => system.OnRun(monoProvider));
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
