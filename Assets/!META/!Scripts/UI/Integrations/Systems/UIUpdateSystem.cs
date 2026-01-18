using System.Runtime.InteropServices;
using BitterECS.Core;

public class UIUpdateSystem : IClientSceneTransitionStart, IClientSceneTransitionComplete
{
    public Priority PrioritySystem => Priority.High;

    public void OnStart()
    {
        UIRootManager.CloseAllPopups();
        VFXService.OnClientSceneTransitionSet(1f);
    }

    public void OnComplete()
    {
        VFXService.OnClientSceneTransitionComplete(() =>
        {
            UIRootManager.OpenPopup<UICornerPopup>();
            if (MobileInputSystem.IsMobile)
                UIRootManager.OpenScreen<UIMobileJoystickScreen>();
            CursorService.LockCursor();
        });
    }
}
