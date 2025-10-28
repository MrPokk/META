using UnityEngine;
using VContainer;

public class TeleportPoint : MonoBehaviour
{
    [SerializeField] private int _floorNumber;

    private TeleportService _teleportService;

    public int FloorNumber => _floorNumber;

    [Inject]
    public void Construct(TeleportService teleportService)
    {
        _teleportService = teleportService;
        _teleportService.RegisterTeleport(this);
    }

    private void OnDestroy()
    {
        _teleportService?.UnregisterTeleport(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ITeleported>(out var teleported))
        {
            UIRootManager.OpenPopup<UITeleportPopup>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ITeleported>(out var teleported))
        {
            UIRootManager.ClosePopup<UITeleportPopup>();
        }
    }
}
