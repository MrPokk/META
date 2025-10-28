using Michsky.MUIP;
using UnityEngine;
using VContainer;

public class UITeleportPopup : UIPopup
{
    [SerializeField] private GameObject _buttonFloorPrefab;
    [SerializeField] private Transform _buttonContainer;

    private TeleportService _teleportService;

    [Inject]
    public void Construct(TeleportService teleportService)
    {
        _teleportService = teleportService;
        _teleportService.OnTeleport += OnTeleportExecuted;

        CreateButtons();
        SetupUI();
    }

    private void OnDestroy()
    {
        if (_teleportService != null)
        {
            _teleportService.OnTeleport -= OnTeleportExecuted;
        }
    }

    private void OnTeleportExecuted(TeleportPoint teleportPoint)
    {
        Close();
    }

    public override void Open()
    {
        base.Open();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public override void Close()
    {
        base.Close();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetupUI()
    {

    }

    private void CreateButtons()
    {
        if (!_buttonContainer || !_buttonFloorPrefab || _teleportService == null) return;

        foreach (var teleportPoint in _teleportService.GetTeleports())
        {
            var buttonObj = Instantiate(_buttonFloorPrefab, _buttonContainer);
            if (buttonObj.TryGetComponent<ButtonManager>(out var manager))
            {
                manager.SetText($"{teleportPoint.FloorNumber}");
                manager.onClick.AddListener(() => _teleportService.ExecuteTeleport(teleportPoint));
            }
        }
    }
}
