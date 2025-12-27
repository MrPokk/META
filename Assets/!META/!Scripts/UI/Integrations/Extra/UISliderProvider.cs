using System;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Events;
using static Michsky.MUIP.RadialSlider;

[RequireComponent(typeof(SliderManager))]
public class UISliderProvider : MonoBehaviour
{
    private SliderManager _sliderManager;
    private SliderEvent _onSubmit;

    [SerializeField] private SaveKey _saveKey = SaveKey.NULL;
    [SerializeField] private int _defaultValue = 30;

    private void Awake()
    {
        _sliderManager ??= GetComponent<SliderManager>();
        if (_saveKey != SaveKey.NULL)
        {
            _sliderManager.mainSlider.value = SaveService.Load(_saveKey, _defaultValue);
            _sliderManager.sliderEvent?.AddListener(OnSetValue);
        }
    }

    private void OnSetValue(float value)
    {
        SaveService.Save(_saveKey, value);
    }

    public void AddListener(UnityAction<float> action)
    {
        _sliderManager.sliderEvent?.AddListener(action);
    }

    public void RemoveListener(UnityAction<float> action)
    {
        _onSubmit?.RemoveListener(action);
    }

    private void OnDestroy()
    {
        _sliderManager.sliderEvent.RemoveAllListeners();
    }
}
