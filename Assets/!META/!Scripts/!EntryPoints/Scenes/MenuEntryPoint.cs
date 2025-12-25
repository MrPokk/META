using VContainer.Unity;

public class MenuEntryPoint : LifetimeScope
{
    private void Start()
    {
        UIRootManager.OpenScreen<UIMainScreen>();
    }
}
