using BitterECS.Core;
using BitterECS.Integration;
using UnityEngine;

public class PlayableRotationSystem : IClientConnectedFixedRun, IClientStart
{
    public Priority PrioritySystem => Priority.High;
    public Camera mainCamera;

    private EcsFilter _ecsFilter = EcsWorld.Get<PlayerPresenter>().Filter()
        .Include<ControllableComponent>()
        .Include<MovingComponent>();

    public void Start()
    {
        mainCamera = Camera.main;
    }

    public void FixedRun()
    {
        var query = _ecsFilter.Collect();

        foreach (var player in query)
        {
            if (player.Provider is not PlayerProvider monoProvider)
                continue;

            if (monoProvider == null || monoProvider.gameObject == null)
                continue;

            if (mainCamera == null)
                continue;

            var cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0;
            monoProvider.transform.rotation = Quaternion.LookRotation(cameraForward).normalized;
        }
    }

}
