using BitterECS.Integration;
using UnityEngine;

[RequireComponent(typeof(MovingComponentProvider))]
public class PlayerProvider : MonoProvider<PlayerPresenter>, ITeleported
{
    public void EnterTeleport(TeleportPoint teleportPoint)
    {
        if (Entity.Has<ControllableComponent>())
            UIRootManager.OpenPopup<UITeleportPopup>();
    }

    public void ExitTeleport(TeleportPoint teleportPoint)
    {
        if (Entity.Has<ControllableComponent>())
            UIRootManager.ClosePopup<UITeleportPopup>();
    }
}
