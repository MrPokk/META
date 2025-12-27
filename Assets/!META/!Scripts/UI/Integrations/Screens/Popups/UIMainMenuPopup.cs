using UnityEngine;

public class UIMainMenuPopup : UIPopup
{
    [SerializeField] private UIButtonProvider _btnGoToGlobalMenu;
    [SerializeField] private UIButtonProvider _btnGoToBackGame;

    public override void Open()
    {
        AddListener();

        UIAnimationComponent
            .UsingAnimation(gameObject)
            .ApplyPresetOpen(UIAnimationPresets.CreateSlideFromRightPreset())
            .PlayOpenAnimation();

        base.Open();
    }

    private void AddListener()
    {
        _btnGoToGlobalMenu.AddListener(OnGoToGlobalMenuButted);
        _btnGoToBackGame.AddListener(OnGoToBackGameButted);
    }

    private void OnGoToBackGameButted()
    {
        UIRootManager.ClosePopup<UIMainMenuPopup>();
    }

    private void OnGoToGlobalMenuButted()
    {
        UIRootManager.CloseAllPopups();
        UIRootManager.OpenScreen<UIMainScreen>();
    }
}
