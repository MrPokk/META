using BitterECS.Core;
using Unity.Cinemachine;
using UnityEngine;

public class PlayableRotationSystem : IClientConnectedFixedRun, IClientStart
{
    public Priority PrioritySystem => Priority.High;
    public Camera mainCamera;

    private EcsFilter _ecsFilter =
    Build.For<PlayerPresenter>()
         .Filter()
         .Include<ControllableComponent>()
         .Include<MovingComponent>()
         .Exclude<CameraEventComponent>();

    private EcsEvent _ecsEvent =
    Build.For<PlayerPresenter>()
         .Event()
         .SubscribeWhere<CameraEventComponent,ControllableComponent>(
            EcsConditions.HasAll<CameraEventComponent,ControllableComponent>,
            OnAddRotationCamera);

    private static void OnAddRotationCamera(EcsEntity entity)
    {
        if (!entity.Has<ControllableComponent>())
        {
            return;
        }

        var monoProvider = entity.Provider as PlayerProvider;
        var cameraPosition = monoProvider.CameraObjectComponent.CinemachineCamera.transform.position;
        var cameraRotation = monoProvider.transform.rotation;
        monoProvider.CameraObjectComponent.CinemachineCamera.ForceCameraPosition(cameraPosition, cameraRotation);
        entity.Remove<CameraEventComponent>();
    }

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
