using BitterECS.Core;

public class UIUpdateSystem : IClientSceneTransitionStart, IClientSceneTransitionComplete
{
    public Priority PrioritySystem => Priority.High;

    public void OnComplete()
    {
        VFXService.OnClientSceneTransitionComplete();
        UIRootManager.OpenScreen<UICornerScreen>();
    }

    public void OnStart()
    {
        UIRootManager.CloseScreen();
        VFXService.OnClientSceneTransitionSet(1f);
    }
}
