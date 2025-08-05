using BitterECS.Core;
using BitterECS.Core.Integration;
using VContainer.Unity;

public class GameplayEntryPoint : LifetimeScope
{
    private void Start()
    {
        var playerPresenter = EcsWorld.Get<PlayerPresenter>();
        playerPresenter.AddEntity<PlayerEntity>()
         .WithLink(EcsUnityViewDatabase.GetInstance<PlayerView>()).Create();


         
    }
}
