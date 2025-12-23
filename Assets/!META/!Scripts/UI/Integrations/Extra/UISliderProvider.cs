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

    private void Awake()
    {
        _sliderManager ??= GetComponent<SliderManager>();
    }

    public void AddListener(UnityAction<float>  action)
    {
        _sliderManager.onValueChanged?.AddListener(action);
    }

    public void RemoveListener(UnityAction<float> action)
    {
        _onSubmit?.RemoveListener(action);
    }

    private void OnDestroy()
    {
        _sliderManager.onValueChanged.RemoveAllListeners();
    }
}
