using DG.Tweening;
using UnityEngine;

public static class UIAnimationPresets
{
    public static UIAnimationPreset CreatePopupPreset()
    {
        return new UIAnimationPreset
        {
            duration = 0.3f,
            easeType = Ease.OutBack,
            startScale = Vector3.zero,
            endScale = Vector3.one,
            startAlpha = 0f,
            endAlpha = 1f
        };
    }

    public static UIAnimationPreset CreateScreenPreset()
    {
        return new UIAnimationPreset
        {
            duration = 0.4f,
            easeType = Ease.OutQuart,
            startScale = Vector3.one,
            endScale = Vector3.one,
            startAlpha = 0f,
            endAlpha = 1f
        };
    }

    public static UIAnimationPreset CreateFadePreset()
    {
        return new UIAnimationPreset
        {
            duration = 0.4f,
            easeType = Ease.OutQuad,
            startScale = Vector3.one,
            endScale = Vector3.one,
            startAlpha = 0f,
            endAlpha = 1f
        };
    }

    public static UIAnimationPreset CreateSlideFromRightPreset()
    {
        return new UIAnimationPreset
        {
            duration = 0.4f,
            easeType = Ease.OutCubic,
            startScale = Vector3.one,
            endScale = Vector3.one,
            startAlpha = 1f,
            endAlpha = 1f
        };
    }

    public static UIAnimationPreset CreateBouncePreset()
    {
        return new UIAnimationPreset
        {
            duration = 0.6f,
            easeType = Ease.OutBounce,
            startScale = Vector3.zero,
            endScale = Vector3.one,
            startAlpha = 0f,
            endAlpha = 1f
        };
    }
}
