using Unity.Cinemachine;
using UnityEngine;

public class CameraObjectComponent : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private CameraTargetComponent _cameraTarget;

    public CinemachineCamera CinemachineCamera { get => _cinemachineCamera; }

    private void Awake()
    {
        _cinemachineCamera ??= GetComponentInChildren<CinemachineCamera>();
        _cameraTarget ??= GetComponentInChildren<CameraTargetComponent>();
    }
}
