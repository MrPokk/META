using System;
using System.Linq;
using BitterECS.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllableSystem : IEcsInitSystem, IEcsDestroySystem
{
    public Priority PrioritySystem => Priority.FIRST_TASK;
    private ControlsConfig _inputs;

    public void Init()
    {
        _inputs = new ControlsConfig();
        _inputs.Enable();
        _inputs.Playable.Move.performed += MovePressingSystem;
        _inputs.Playable.Move.canceled += MoveUnPressingSystem;
        //_inputs.UI.Navigate.performed += NavigatePressingSystem; TODO: Make optimized navigation
        _inputs.UI.Cancel.performed += CancelPressingSystem;
    }

    private void CancelPressingSystem(InputAction.CallbackContext context)
    {
        CursorService.SwitchCursor();
    }

    private void NavigatePressingSystem(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }

        var isApplyScreen = ApplyScreen();
        if (!isApplyScreen)
        {
            return;
        }
        var isApplyPopup = ApplyPopup();
        if (!isApplyPopup)
        {
            return;
        }
    }

    private bool ApplyPopup()
    {
        if (!UIRootManager.GetCurrentPopups.Any())
        {
            return false;
        }

        var popupFirst = UIRootManager.GetCurrentPopups.First();

        if (popupFirst == null || popupFirst.Equals(null))
        {
            return false;
        }

        if (popupFirst is not UIPopup popupCast)
        {
            return false;
        }

        if (popupCast == null || popupCast.Equals(null) ||
            (popupCast is MonoBehaviour monoBehaviour && monoBehaviour == null))
        {
            return false;
        }

        var isNavigation = popupCast.TryGetComponent<UINavigationComponent>(out var navigationComponent);
        if (!isNavigation)
        {
            return false;
        }

        navigationComponent.SetFirstSelectedButton();
        return true;
    }

    private bool ApplyScreen()

    {
        var currentScreen = UIRootManager.GetCurrentScreen;

        if (currentScreen == null || currentScreen.Equals(null))
        {
            return false;
        }

        if (currentScreen is not UIScreen uiScreen)
        {
            return false;
        }

        if (uiScreen == null || uiScreen.Equals(null) ||
            (uiScreen is MonoBehaviour monoBehaviour && monoBehaviour == null))
        {
            return false;
        }

        var isNavigation = uiScreen.TryGetComponent<UINavigationComponent>(out var navigationComponent);
        if (!isNavigation)
        {
            return false;
        }

        navigationComponent.SetFirstSelectedButton();
        return true;
    }
    private void MoveUnPressingSystem(InputAction.CallbackContext context)
    {
        var controllableEntity = Build.For<PlayerPresenter>()
        .Filter()
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
        var controllableEntity = Build.For<PlayerPresenter>()
        .Filter()
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
        if (_inputs == null)
        {
            return;
        }

        _inputs.Playable.Move.performed -= MovePressingSystem;
        _inputs.Playable.Move.canceled -= MoveUnPressingSystem;
        _inputs.UI.Navigate.performed -= NavigatePressingSystem;
        _inputs.Disable();
    }
}
