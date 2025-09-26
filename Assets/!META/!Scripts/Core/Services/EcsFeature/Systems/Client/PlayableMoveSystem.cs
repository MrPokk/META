using BitterECS.Core;
using BitterECS.Integration;
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

            if (entity.Provider is MonoProvider monoProvider)
            {
                var direction = new Vector3(controllableComponent.input.x, 0, controllableComponent.input.y);
                monoProvider.gameObject.transform.Translate(direction * movingComponent.speed * Time.deltaTime);
            }
        }
    }
}
