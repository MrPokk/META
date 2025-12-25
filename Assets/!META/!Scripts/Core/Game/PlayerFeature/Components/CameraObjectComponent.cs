using Unity.Cinemachine;
using UnityEngine;

public class CameraObjectComponent : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    private CinemachineInputAxisController _cinemachineInputAxisController;
    [SerializeField] private CameraTargetComponent _cameraTarget;

    public CinemachineCamera CinemachineCamera { get => _cinemachineCamera; }

    private void Awake()
    {
        _cinemachineCamera ??= GetComponentInChildren<CinemachineCamera>();
        _cinemachineInputAxisController ??= GetComponentInChildren<CinemachineInputAxisController>();
        _cameraTarget ??= GetComponentInChildren<CameraTargetComponent>();

        SetMultipleAxisController();
    }

    private void SetMultipleAxisController()
    {
        var multipleAxisController = _cinemachineInputAxisController.Controllers;
        foreach (var axis in multipleAxisController)
        {
            var sensitivity = SaveService.Load<float>(SaveKey.Sensitivity) / 100.0f;
            var sensitivityClamp = Mathf.Clamp(sensitivity, 0.1f, 1f);
            axis.Input.Gain *= sensitivityClamp;
        }
    }
}
