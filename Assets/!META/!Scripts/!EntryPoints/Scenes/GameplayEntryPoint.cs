using BitterECS.Core;
using BitterECS.Core.Integration;
using VContainer.Unity;

public class GameplayEntryPoint : LifetimeScope
{
    public Priority PrioritySystem => Priority.Medium;

    private void Start()
    {
        var playerPresenter = EcsWorld.Get<PlayerPresenter>();
        playerPresenter.AddEntity<PlayerEntity>()
         .WithLink(EcsUnityViewDatabase.GetInstance<PlayerView>())
         .WithComponent<NetworkChapterComponent>(new()).Create();
    }
}
