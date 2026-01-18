using System;
using BitterECS.Core;
using Cysharp.Threading.Tasks;
using Gley.Localization;
using UnityEngine;

public class UIChatPopup : UIPopup
{
    [SerializeField] private UIInputFieldProvider _inputFieldProvider;
    [SerializeField] private UIButtonProvider _btnSubmit;
    [SerializeField] private UIChatContent _chatContent;

    [Header("Setting")]
    [SerializeField] private Color _colorOwner;
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
        _inputFieldProvider.AddListener(OnInputFieldSubmitted);
        _inputFieldProvider.AddListenerToSelected(OnStartSelected);
        _inputFieldProvider.AddListenerToEndSelected(OnEndSelected);
        _btnSubmit.AddListener(OnButtonSubmit);
    }

    private void OnEndSelected()
    {
        GeneralInputSystem.EnablePlayable();
    }

    private void OnStartSelected()
    {
        GeneralInputSystem.DisablePlayable();
    }

    private void OnButtonSubmit() => _inputFieldProvider.OnSubmit();

    private void OnInputFieldSubmitted()
    {
        ChatNetworkProvider.SendChatMessage(_inputFieldProvider.OnSubmitText);
    }

    public void AddContent(ChatMessage message)
    {
        if (NetworkUtility.IsSenderToOwned(message.ownerId))
        {
            AddOwnerMessage(message);
        }
        else
        {
            AddStandardMessage(message);
        }
    }

    private void AddStandardMessage(ChatMessage message)
    {
        var makeMessage = $"{message.sender}: {message.message}";
        Instantiate(_chatContent.chatContentItem, _chatContent.transform).SetText(makeMessage);
    }

    public void AddOwnerMessage(ChatMessage message)
    {
        var hexRGB = ColorUtility.ToHtmlStringRGB(_colorOwner);
        var makeMessage = $"<color=#{hexRGB}>{API.GetText(WordIDs.NameOwnerID)}:</color> {message.message}";
        Instantiate(_chatContent.chatContentItem, _chatContent.transform).SetText(makeMessage);
    }
}

public class UIAddContentMessage : IClientChatMessage
{
    public Priority PrioritySystem => Priority.Low;

    public void OnMessage(ChatMessage message)
    {
        var isOpenedChat = UIRootManager.TryGetOpenedPopup<UIChatPopup>(out var uiChat);
        if (isOpenedChat)
        {
            uiChat.AddContent(message);
        }
    }
}
