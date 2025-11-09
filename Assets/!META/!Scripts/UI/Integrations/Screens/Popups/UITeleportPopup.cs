using Michsky.UI.Heat;
using UnityEngine;
using VContainer;

public class UITeleportPopup : UIPopup
{
    [SerializeField] private ButtonManager _buttonFloorPrefab;
    [SerializeField] private Transform _buttonContainer;

    private TeleportService _teleportService;

    [Inject]
    public void Construct(TeleportService teleportService)
    {
        _teleportService = teleportService;
        _teleportService.OnTeleport += OnTeleportExecuted;

        CreateButtons();
    }

    private void OnTeleportExecuted(TeleportPoint teleportPoint)
    {
        Close();
        SceneNetworkProvider.ChangeScene(teleportPoint.SceneType);
    }

    public override void Open()
    {
        base.Open();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public override void Close()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _teleportService.OnTeleport -= OnTeleportExecuted;

        base.Close();
    }

    private void CreateButtons()
    {
        if (!_buttonContainer || !_buttonFloorPrefab || _teleportService == null) return;

        foreach (var teleportPoint in _teleportService.GetTeleports())
        {
            var buttonObj = Instantiate(_buttonFloorPrefab, _buttonContainer);
            buttonObj.SetText($"{teleportPoint.FloorNumber}");
            buttonObj.onClick.AddListener(() => _teleportService.ExecuteTeleport(teleportPoint));
        }
    }
}
