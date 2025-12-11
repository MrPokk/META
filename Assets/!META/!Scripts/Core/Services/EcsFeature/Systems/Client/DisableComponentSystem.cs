using BitterECS.Core;
using BitterECS.Integration;
using UnityEngine;

public class DisableComponentSystem : IClientConnected
{
    public Priority PrioritySystem => Priority.Low;

    private EcsFilter _ecsFilter =
    EcsWorld.Get<PlayerPresenter>().Filter()
        .Include<MovingComponent>()
        .Exclude<ControllableComponent>();

    public void Connect()
    {
        Debug.Log("T");

        var query = _ecsFilter.Collect();
        foreach (var entity in query)
        {
            var monoProvider = entity.Provider as PlayerProvider;
            var cameraObject = monoProvider.CameraObjectComponent;

            Object.Destroy(cameraObject.gameObject);
        }
    }
}

