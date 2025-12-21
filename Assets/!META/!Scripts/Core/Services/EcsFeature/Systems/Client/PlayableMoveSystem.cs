using System;
using BitterECS.Core;
using UnityEngine;

public partial class PlayableMoveSystem : IClientConnectedFixedRun
{
    public Priority PrioritySystem => Priority.High;

    private EcsFilter _ecsFilter =
    Build.For<PlayerPresenter>()
        .Filter()
        .Include<MovingComponent>()
        .Include<StateComponent>();

    public void FixedRun()
    {
        MovePlayer(ref _ecsFilter);
    }

    private void MovePlayer(ref EcsFilter query)
    {
        foreach (var entity in query)
        {
            if (!IsPlayerValid(entity, out var monoProvider))
            {
                continue;
            }

            ref var movingComponent = ref entity.Get<MovingComponent>();
            ref var stateComponent = ref entity.Get<StateComponent>();

            var isControllable = entity.Has<ControllableComponent>();

            UpdateMovementState(ref movingComponent, ref stateComponent, monoProvider);

            if (isControllable)
            {
                ProcessControllablePlayer(entity, monoProvider, ref movingComponent, ref stateComponent);
            }
        }
    }

    private static void UpdateMovementState(ref MovingComponent moving,
        ref StateComponent state, PlayerProvider provider)
    {
        var current = provider.transform.position;

        if (moving.lastPosition == Vector3.zero)
        {
            moving.lastPosition = current;
            SetState(ref state, 0f, m => m > 0.01f);
            return;
        }

        var diff = current - moving.lastPosition;
        diff.y = 0; 

        SetState(ref state, diff.magnitude, m => m > 0.01f);
        moving.lastPosition = current;
    }

    private static void ProcessControllablePlayer(EcsEntity entity, PlayerProvider monoProvider,
        ref MovingComponent movingComponent, ref StateComponent stateComponent)
    {
        ref var controllableComponent = ref entity.Get<ControllableComponent>();
        GetPlayerDirection(monoProvider, out var playerForward, out var playerRight);

        var directionTo = (playerForward * controllableComponent.input.y +
                         playerRight * controllableComponent.input.x).normalized;

        monoProvider.CharacterController.SimpleMove(directionTo * movingComponent.speed);

        SetState(ref stateComponent, directionTo, dir => dir != Vector3.zero);
    }

    private static bool IsPlayerValid(EcsEntity entity, out PlayerProvider outMonoProvider)
    {
        outMonoProvider = null;

        if (entity.Provider is not PlayerProvider monoProvider)
        {
            return false;
        }

        if (monoProvider == null || monoProvider.gameObject == null)
        {
            return false;
        }

        if (monoProvider.CharacterController == null)
        {
            return false;
        }

        outMonoProvider = monoProvider;
        return true;
    }

    private static void SetState<T>(ref StateComponent stateComponent, T movementData, Predicate<T> isMovingPredicate)
    {
        stateComponent.state = isMovingPredicate.Invoke(movementData)
            ? StateComponent.State.Moving
            : StateComponent.State.Idle;
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
