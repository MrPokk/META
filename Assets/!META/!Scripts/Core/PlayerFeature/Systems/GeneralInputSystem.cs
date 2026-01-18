using System;
using System.Linq;
using BitterECS.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllableSystem : IEcsInitSystem, IEcsDestroySystem
{
    public Priority PrioritySystem => Priority.FIRST_TASK;
    private static ControlsConfig s_inputs;

    public void Init()
    {
        s_inputs = new ControlsConfig();
        s_inputs.Enable();
        s_inputs.Playable.Move.performed += NavigationPlayable.MovePressingSystem;
        s_inputs.Playable.Move.canceled += NavigationPlayable.MoveUnPressingSystem;
        s_inputs.UI.Cancel.performed += NavigationUI.CancelPressingSystem;
        //s_inputs.UI.Navigate.performed += NavigationUI.NavigatePressingSystem; TODO: Make optimized navigation and fix bug with UI
    }

    public static void DisablePlayable() => s_inputs.Playable.Disable();
    public static void EnablePlayable() => s_inputs.Playable.Enable();

    public void Destroy()
    {
        if (s_inputs == null)
        {
            return;
        }

        s_inputs.Playable.Move.performed -= NavigationPlayable.MovePressingSystem;
        s_inputs.Playable.Move.canceled -= NavigationPlayable.MoveUnPressingSystem;
        s_inputs.UI.Cancel.performed -= NavigationUI.CancelPressingSystem;
        //s_inputs.UI.Navigate.performed -= NavigationUI.NavigatePressingSystem;
    }
}

public static class NavigationPlayable
{
    private static EcsFilter.Enumerator EcsEntities =>
     Build.For<PlayerPresenter>()
          .Filter()
          .Include<ControllableComponent>()
          .Collect();

    public static void MoveUnPressingSystem(InputAction.CallbackContext _)
    {
        foreach (var entity in EcsEntities)
        {
            entity.Get<ControllableComponent>().input = Vector2.zero;
        }
    }

    public static void MovePressingSystem(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();

        foreach (var entity in EcsEntities)
        {
            entity.Get<ControllableComponent>().input = direction;
        }
    }
}

public static class NavigationUI
{
    public static void CancelPressingSystem(InputAction.CallbackContext _)
    {
        CursorService.SwitchCursor();
    }

    public static void NavigatePressingSystem(InputAction.CallbackContext _)
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

    private static bool ApplyPopup()
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

    private static bool ApplyScreen()
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
}
