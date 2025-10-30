using UnityEngine;
using VContainer;

public class TeleportPoint : MonoBehaviour
{
    [SerializeField] private int _floorNumber;
    [SerializeField] private SceneTypes _sceneType;

    private TeleportService _teleportService;

    public int FloorNumber => _floorNumber;
    public SceneTypes SceneType => _sceneType;

    [Inject]
    public void Construct(TeleportService teleportService)
    {
        _teleportService = teleportService;
    }

    private void Start()
    {
        _teleportService?.RegisterTeleport(this);
    }

    private void OnDestroy()
    {
        _teleportService?.UnregisterTeleport(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ITeleported>(out var teleported))
        {
            teleported.EnterTeleport(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<ITeleported>(out var teleported))
        {
            teleported.ExitTeleport(this);
        }
    }
}
