using BitterECS.Core;
using BitterECS.Integration;
using UnityEngine;

public class PlayableRotationSystem : IClientConnectedFixedRun, IClientStart
{
    public Priority PrioritySystem => Priority.High;
    public Camera mainCamera;

    public void Start()
    {
        mainCamera = Camera.main;
    }

    public void FixedRun()
    {
        var query = EcsWorld.Get<PlayerPresenter>().Filter()
            .Include<ControllableComponent>()
            .Include<MovingComponent>()
            .Collect();

        foreach (var entity in query)
        {
            if (entity.Provider is PlayerProvider monoProvider)
            {
                if (mainCamera != null)
                {
                    var cameraForward = mainCamera.transform.forward;
                    cameraForward.y = 0;
                    monoProvider.transform.rotation = Quaternion.LookRotation(cameraForward).normalized;
                }
            }
        }
    }

}
