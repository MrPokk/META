using System.Runtime.InteropServices;
using BitterECS.Core;
using UnityEngine;
using UnityEngine.InputSystem;

public class MobileInputSystem : IClientConnected, IClientConnectedRun
{
    public Priority PrioritySystem => Priority.FIRST_TASK;

    private EcsFilter.Enumerator EcsEntities =>
    Build.For<PlayerPresenter>()
         .Filter()
         .Include<ControllableComponent>()
         .Collect();

    private static bool _isMobile = false;
    public static bool IsMobile => _isMobile;

    public static Joystick Joystick { get; set; }

    public static Vector2 GetJoystickDirection()
    {
        return Joystick != null && IsMobile ? Joystick.Direction : Vector2.zero;
    }

#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern bool IsMobile();
#endif

    public void Connect()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
      _isMobile = IsMobile();
#endif
    }

    public void Run()
    {
        if (!IsMobile)
            return;

        foreach (var entity in EcsEntities)
        {
            entity.Get<ControllableComponent>().input = GetJoystickDirection();
        }
    }
}
