
using Michsky.MUIP;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UITeleport : UIPopup
{
    [Header("UI References")]
    [SerializeField] private GameObject _buttonFloorPrefab;
    [SerializeField] private Transform _buttonContainer;

    [Header("UI Customization")]
    [SerializeField] private float _buttonSpacing = 10f;

    private Dictionary<TeleportModel, ButtonManager> _allButtonTeleport = new();

    public void Init(TeleportPresenter teleportPresenter)
    {
        SetupUI();
        CreateButtons(teleportPresenter);
        Close();

        TeleportPresenter.OnTeleported += ChangeUI;
    }

    private void ChangeUI(TeleportModel teleport)
    {
        // _allButtonTeleport[teleport].is;
    }

    public override void Open()
    {
        gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public override void Close()
    {
        gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void SetupUI()
    {
        if (_buttonContainer)
        {
            var containerRect = _buttonContainer.GetComponent<RectTransform>();
            if (containerRect)
            {
                containerRect.anchorMin = new Vector2(0, 0);
                containerRect.anchorMax = new Vector2(1, 1);
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
            }

            var existingVertical = _buttonContainer.GetComponent<VerticalLayoutGroup>();
            var existingHorizontal = _buttonContainer.GetComponent<HorizontalLayoutGroup>();
            if (existingVertical) DestroyImmediate(existingVertical);
            if (existingHorizontal) DestroyImmediate(existingHorizontal);

            var verticalLayout = _buttonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayout.spacing = _buttonSpacing;
            verticalLayout.childAlignment = TextAnchor.LowerLeft;
            verticalLayout.childControlWidth = false;
            verticalLayout.childControlHeight = false;
            verticalLayout.childForceExpandWidth = false;
            verticalLayout.childForceExpandHeight = false;
            verticalLayout.padding = new RectOffset(5, 5, 5, 5);

        }
    }

    private void CreateButtons(TeleportPresenter teleportPresenter)
    {
        if (!_buttonContainer || !_buttonFloorPrefab)
        {
            Debug.LogError("buttonContainer или buttonPrefab не назначены!");
            return;
        }
        foreach (var vModel in teleportPresenter.GetTeleports())
        {
            var buttonObj = Instantiate(_buttonFloorPrefab, _buttonContainer);
            var isButtonManager = buttonObj.TryGetComponent<ButtonManager>(out var manager);
            if (isButtonManager)
            {
                manager.buttonText = vModel.Key.floorNumber.ToString();
                manager.onClick.AddListener(() => teleportPresenter.Teleported(vModel.Value));

                _allButtonTeleport.TryAdd(vModel.Value, manager);
            }
            else
            {
                Debug.LogError("ButtonManager не найден на кнопке!");
            }
        }
    }
}
