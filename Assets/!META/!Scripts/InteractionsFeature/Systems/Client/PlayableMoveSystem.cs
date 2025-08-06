using BitterECS.Core;
using UnityEngine;

public class MoveSystem : IEcsRunSystem
{
    public Priority PrioritySystem => Priority.High;

    public void Run()
    {
        var controllableEntity = EcsWorld.Get<PlayerPresenter>().Filter()
        .Include<NetworkChapterComponent>()
        .Include<ControllableComponent>()
        .Include<MoveComponent>()
        .Collect();

        foreach (var entity in controllableEntity)
        {
            ref var direction = ref entity.Get<ControllableComponent>().input;
            ref var speed = ref entity.Get<MoveComponent>().speed;
            if (entity.Get<ViewComponent>().current is EcsUnityView unityView)
            {
                var directionMovement = new Vector3(direction.x, 0, direction.y);
                unityView?.transform.Translate(directionMovement * speed * Time.deltaTime);
            }
        }
    }
}
