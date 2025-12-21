using BitterECS.Core;
using UnityEngine;

public class PlayableRotationSystem : IClientConnectedFixedRun, IClientStart
{
    public Priority PrioritySystem => Priority.High;
    public Camera mainCamera;

    private EcsFilter _ecsFilter = 
    Build.For<PlayerPresenter>()
         .Filter()
         .Include<ControllableComponent>()
         .Include<MovingComponent>();

    public void Start()
    {
        mainCamera = Camera.main;
    }

    public void FixedRun()
    {
        foreach (var player in _ecsFilter)
        {
            if (player.Provider is not PlayerProvider monoProvider)
            {
                continue;
            }

            if (monoProvider == null || monoProvider.gameObject == null)
            {
                continue;
            }

            if (mainCamera == null)
            {
                continue;
            }

            var cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0;
            monoProvider.transform.rotation = Quaternion.LookRotation(cameraForward).normalized;
        }
    }

}
