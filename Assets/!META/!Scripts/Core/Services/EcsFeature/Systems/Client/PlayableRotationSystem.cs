using BitterECS.Core;
using BitterECS.Integration;
using UnityEngine;

public class PlayableRotationSystem : IClientConnectedRun, IClientConnected
{
    public Priority PrioritySystem => Priority.High;
    public Camera mainCamera;

    public void Connect()
    {
        mainCamera = Camera.main;
    }

    public void Run()
    {
        var query = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Collect();

        foreach (var entity in query)
        {
            if (entity.Provider is MonoProvider monoProvider)
            {
                if (mainCamera != null)
                {
                    var cameraForward = mainCamera.transform.forward;
                    monoProvider.transform.rotation = Quaternion.LookRotation(cameraForward).normalized;
                }
            }
        }
    }
}
