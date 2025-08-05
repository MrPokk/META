using System;
using System.Collections.Generic;
using BitterECS.Core;
using UnityEngine;
using UnityEngine.InputSystem;


public class ControllableSystem : IEcsInitSystem, IEcsDestroySystem
{
    public Priority PrioritySystem => Priority.Medium;
    private ControlsConfig _inputs;

    public void Init()
    {
        _inputs = new ControlsConfig();
        _inputs.Enable();
        _inputs.Playable.Move.performed += MovePressingSystem;
        _inputs.Playable.Move.canceled += MoveUnPressingSystem;
    }

    private void MoveUnPressingSystem(InputAction.CallbackContext context)
    {
        var controllableEntity = EcsWorld.Get<PlayerPresenter>().Filter()
        .Include<ControllableComponent>()
        .Collect();

        foreach (var entity in controllableEntity)
        {
            ref var controllableComponent = ref entity.Get<ControllableComponent>();
            controllableComponent.input = Vector2.zero;
        }
    }

    private void MovePressingSystem(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();
        var controllableEntity = EcsWorld.Get<PlayerPresenter>().Filter()
        .Include<ControllableComponent>()
        .Collect();

        foreach (var entity in controllableEntity)
        {
            ref var controllableComponent = ref entity.Get<ControllableComponent>();
            controllableComponent.input = direction;
        }
    }

    public void Destroy()
    {
        _inputs.Playable.Move.performed -= MovePressingSystem;
        _inputs.Playable.Move.canceled -= MoveUnPressingSystem;
    }
}
