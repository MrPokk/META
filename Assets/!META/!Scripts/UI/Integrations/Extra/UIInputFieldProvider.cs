using System;
using Michsky.MUIP;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CustomInputField))]
public class UIInputFieldProvider : MonoBehaviour, IUIProvider
{
    [SerializeField] private CustomInputField _inputFieldProvider;
    [SerializeField] private bool _isClearOnSubmit = true;
    public string OnSubmitText => _inputFieldProvider.inputText.text;
    private void InitializeComponents()
    {
        _inputFieldProvider ??= GetComponent<CustomInputField>();
    }

    private void Awake()
    {
        InitializeComponents();
    }

    public void AddListener(UnityAction action)
    {
        _inputFieldProvider.onSubmit.AddListener(action);
    }

    public void RemoveListener(UnityAction action)
    {
        _inputFieldProvider.onSubmit.RemoveListener(() => ConversionAction(action));
    }

    public void OnSubmit()
    {
        _inputFieldProvider.onSubmit?.Invoke();
        ClearOnSubmit();
    }

    private void ConversionAction(UnityAction action)
    {
        action();
        ClearOnSubmit();
    }

    private void ClearOnSubmit()
    {
        if (_isClearOnSubmit)
            _inputFieldProvider.inputText.text = "";
    }

    private void OnDestroy()
    {
        _inputFieldProvider.onSubmit?.RemoveAllListeners();
        _inputFieldProvider.onSubmit?.RemoveListener(ClearOnSubmit);
    }
}
