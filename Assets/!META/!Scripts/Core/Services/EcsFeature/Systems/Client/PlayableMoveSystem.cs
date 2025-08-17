using BitterECS.Core;
using UnityEngine;

public class PlayableMoveSystem : IClientConnectedRun
{
    public Priority PrioritySystem => Priority.High;

    public void Run()
    {
        var query = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Collect();

        foreach (var entity in query)
        {
            ref var movingComponent = ref entity.Get<MovingComponent>();
            ref var controllableComponent = ref entity.Get<ControllableComponent>();

            var viewGameObject = EcsLinker.GetView<PlayerView>(entity);
            if (viewGameObject == null)
                continue;

            var direction = new Vector3(controllableComponent.input.x, 0, controllableComponent.input.y);
            viewGameObject.transform.Translate(direction * movingComponent.speed * Time.deltaTime);
        }
    }
}
