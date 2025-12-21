using System.Collections.Generic;
using Gley.Localization;
using UnityEngine;
using VContainer;

public class UITeleportPopup : UIPopup
{

    private List<UIButtonProvider> _buttons = new();
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
        base.Open();
        
        UINavigationComponent
              .UsingNavigation(gameObject)
              .ApplyFirstSelected()
              .ApplyNavigation(_buttons, true);


        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

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
            _buttons.Add(buttonObj);
        }
    }
}
