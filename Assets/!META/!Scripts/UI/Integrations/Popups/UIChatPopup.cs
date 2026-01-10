using System;
using BitterECS.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIChatPopup : UIPopup
{
    [SerializeField] private UIInputFieldProvider _inputFieldProvider;
    [SerializeField] private UIButtonProvider _btnSubmit;
    [SerializeField] private UIChatContent _chatContent;
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
        ControllableSystem.EnablePlayable();
    }

    private void OnStartSelected()
    {
        ControllableSystem.DisablePlayable();
    }

    private void OnButtonSubmit() => _inputFieldProvider.OnSubmit();

    private void OnInputFieldSubmitted()
    {
        MessageNetworkProvider.SendChatMessage(_inputFieldProvider.OnSubmitText).Forget();
    }

    public void AddContent(ChatMessage message)
    {
        Instantiate(_chatContent.chatContentItem, _chatContent.transform).SetText(message.message);
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
