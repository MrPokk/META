using BitterECS.Core;
using UnityEngine;

public class PlayableMoveSystem : IClientConnectedRun
{
    public Priority PrioritySystem => Priority.High;

    public void Run()
    {
        var controllableEntity = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<NetworkChapterComponent>()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Collect();

        foreach (var entity in controllableEntity)
        {
            ref var direction = ref entity.Get<ControllableComponent>().input;
            ref var speed = ref entity.Get<MovingComponent>().speed;
            ref var viewComponent = ref entity.Get<ViewComponent>();

            var ecsUnityView = (MonoBehaviour)viewComponent.current;
            if (ecsUnityView != null)
            {
                var directionMovement = new Vector3(direction.x, 0, direction.y);
                ecsUnityView.transform.Translate(directionMovement * speed * Time.deltaTime);
            }
        }
    }
}
