using System;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SwitchManager))]
public class UISwitchProvider : MonoBehaviour
{
    [SerializeField] private SwitchManager _switchManager;

    [SerializeField] private SaveKey _saveKey = SaveKey.NULL;
    [SerializeField] private bool _defaultValue = true;

    private void Awake()
    {
        _switchManager ??= GetComponent<SwitchManager>();
        if (_saveKey != SaveKey.NULL)
        {
            _switchManager.isOn = SaveService.Load(_saveKey, _defaultValue);

            if (_switchManager.isOn)
            {
                _switchManager.SetOn();
            }
            else
            {
                _switchManager.SetOff();
            }

            _switchManager.onValueChanged?.AddListener(OnSetValue);
        }
    }

    private void OnSetValue(bool value)
    {
        SaveService.Save(_saveKey, value);
    }

    public void AddListener(UnityAction<bool> action)
    {
        _switchManager?.onValueChanged?.AddListener(action);
    }

    public void RemoveListener(UnityAction<bool> action)
    {
        _switchManager?.onValueChanged?.RemoveListener(action);
    }

    private void OnDestroy()
    {
        _switchManager?.onValueChanged?.RemoveAllListeners();
    }
}
