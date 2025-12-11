using Gley.Localization;
using UnityEngine;
using VContainer;

public class UITeleportPopup : UIPopup
{
    [SerializeField] private UIButtonProvider _buttonFloorPrefab;
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
        SceneNetworkProvider.ChangeScene(teleportPoint.SceneType);
        Close();
    }

    public override void Open()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        base.Open();

        UIAnimationComponent
        .UsingAnimation(gameObject)
        .ApplyPresetOpen(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayOpenAnimation();
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

        var sortTeleport = _teleportService.GetSortTeleports((t, t2) => t.FloorNumber - t2.FloorNumber);
        foreach (var teleportPoint in sortTeleport)
        {
            var buttonObj = Instantiate(_buttonFloorPrefab, _buttonContainer);
            var textTeleport = $"{API.GetText(WordIDs.FloorID)}: {teleportPoint.FloorNumber}";
            buttonObj.SetText(textTeleport);
            buttonObj.AddListener(() => _teleportService.ExecuteTeleport(teleportPoint));
        }
    }
}
