using System;
using System.Collections.Generic;
using Gley.Localization;
using Michsky.MUIP;
using UnityEngine;

public class UISettingLanguageScreen : UIScreen
{
    [SerializeField] private UISelectorProvider _slLanguage;
    [SerializeField] private UIButtonProvider _btnGoToBack;

    public override void Open()
    {
        AddListener();
        UIAnimationComponent.UsingAnimation(gameObject)
        .ApplyPreset(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayOpenAnimation();
        base.Open();
    }

    private void AddListener()
    {
        _btnGoToBack.AddListener(OnGoToBackButton);
    }

    private void RemoveListener()
    {
        _btnGoToBack.RemoveListener(OnGoToBackButton);
    }

    public override void Close()
    {
        RemoveListener();
        UIAnimationComponent.UsingAnimation(gameObject)
        .ApplyPreset(UIAnimationPresets.CreateSlideFromRightPreset())
        .PlayCloseAnimation();
        base.Close();
    }

    private void OnGoToBackButton() => UIRootManager.OpenScreen<UISettingScreen>();

}

[RequireComponent(typeof(HorizontalSelector))]
public class UISelectorProvider : LocalizedUIElement
{
    [SerializeField] HorizontalSelector _horizontalSelector;

    [SerializeField] List<WordIDs> _wordIDs;

    protected override void InitializeComponents()
    {
        _horizontalSelector ??= GetComponent<HorizontalSelector>();
        _horizontalSelector.items.Clear();

    }

    private void OnSelection()
    {
       
    }

    public override void SetText(string text)
    {
        foreach (var word in _wordIDs)
        {
            _horizontalSelector.CreateNewItem(API.GetText(word));
        }
        _horizontalSelector.UpdateUI();
    }

}
