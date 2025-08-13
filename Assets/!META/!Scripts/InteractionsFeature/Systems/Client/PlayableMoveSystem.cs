using System.Collections.Generic;
using BitterECS.Core;
using Mirror;
using UnityEngine;

public class PlayableMoveSystem : IClientConnectedRun
{
    private Vector3 _lastSentPosition;
    private const float POSITION_THRESHOLD = 0.1f;
    public Priority PrioritySystem => Priority.High;

    public void Run()
    {
        var query = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<NetworkSyncComponent>()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Include<TransformComponent>()
            .Include<ViewComponent>()
            .Collect();

        ProcessMovableEntities(query);
    }

    private void ProcessMovableEntities(EcsFilter.FilterEnumerator query)
    {
        foreach (var entity in query)
        {
            ref readonly var input = ref entity.Get<ControllableComponent>();
            ref readonly var movement = ref entity.Get<MovingComponent>();
            ref var transform = ref entity.Get<TransformComponent>();
            var view = entity.Get<ViewComponent>().current as MonoBehaviour;

            if (view == null)
                continue;

            MoveEntity(view, input, movement, ref transform);
            TrySyncPosition(entity, ref transform);
        }
    }

    private void MoveEntity(
        MonoBehaviour view,
        in ControllableComponent input,
        in MovingComponent movement,
        ref TransformComponent transform)
    {
        Vector3 direction = new Vector3(input.input.x, 0f, input.input.y);
        view.transform.Translate(direction * movement.speed * Time.deltaTime);
        transform.position = view.transform.position;
    }

    private void TrySyncPosition(EcsEntity entity, ref TransformComponent transform)
    {
        if (ShouldSyncPosition(transform.position))
        {
            SyncTransformNetworkProvider.SendRequest(transform, new(entity.GetType()), entity.Get<NetworkSyncComponent>());
            _lastSentPosition = transform.position;
        }
    }

    private bool ShouldSyncPosition(Vector3 currentPosition)
    {
        return Vector3.Distance(_lastSentPosition, currentPosition) > POSITION_THRESHOLD;
    }
}
