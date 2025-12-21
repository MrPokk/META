using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System;
using System.Collections;

[Serializable]
public class UIAnimationPreset
{
    public float duration = 0.3f;
    public Ease easeType = Ease.OutBack;
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;
    public float startAlpha = 0f;
    public float endAlpha = 1f;
}

public class UIAnimationComponent : MonoBehaviour
{
    [Header("Animation Presets")]
    [SerializeField] private UIAnimationPreset _openPreset = new();
    [SerializeField] private UIAnimationPreset _closePreset = new();

    [Header("References (Optional)")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private GraphicRaycaster _raycast;

    private Coroutine _currentCoroutine;
    private Sequence _currentAnimation;

    private void Awake()
    {
        _canvasGroup ??= GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        _rectTransform ??= GetComponent<RectTransform>();
        _raycast = GetComponent<GraphicRaycaster>();

        SetInitialState();
    }

    private void SetInitialState()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = _openPreset.startAlpha;
            _canvasGroup.blocksRaycasts = false;
        }

        if (_rectTransform != null)
        {
            _rectTransform.localScale = _openPreset.startScale;
        }
    }

    public Coroutine PlayOpenAnimationCoroutine(Action onComplete = null)
    {
        CancelCurrentCoroutine();
        return _currentCoroutine = StartCoroutine(OpenAnimationRoutine(onComplete));
    }

    public Coroutine PlayCloseAnimationCoroutine(Action onComplete = null)
    {
        CancelCurrentCoroutine();
        return _currentCoroutine = StartCoroutine(CloseAnimationRoutine(onComplete));
    }

    private IEnumerator OpenAnimationRoutine(Action onComplete = null)
    {
        if (this == null || gameObject == null)
        {
            yield break;
        }

        CancelCurrentAnimation();

        if (_raycast != null) _raycast.enabled = false;
        gameObject.SetActive(true);

        _currentAnimation = DOTween.Sequence();

        if (_canvasGroup != null)
        {
            _currentAnimation.Join(
                _canvasGroup.DOFade(_openPreset.endAlpha, _openPreset.duration)
                    .SetEase(_openPreset.easeType)
            );
        }

        if (_rectTransform != null)
        {
            _currentAnimation.Join(
                _rectTransform.DOScale(_openPreset.endScale, _openPreset.duration)
                    .SetEase(_openPreset.easeType)
            );
        }

        _currentAnimation.OnComplete(() =>
        {
            if (_canvasGroup != null) _canvasGroup.blocksRaycasts = true;
            if (_raycast != null) _raycast.enabled = true;
        });

        _currentAnimation.Play();

        while (_currentAnimation != null && _currentAnimation.IsActive() && _currentAnimation.IsPlaying())
        {
            yield return null;
        }

        onComplete?.Invoke();
        _currentCoroutine = null;
    }

    private IEnumerator CloseAnimationRoutine(Action onComplete = null)
    {
        if (this == null || gameObject == null)
        {
            yield break;
        }

        CancelCurrentAnimation();

        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;
        if (_raycast != null) _raycast.enabled = false;

        _currentAnimation = DOTween.Sequence();

        if (_canvasGroup != null)
        {
            _currentAnimation.Join(
                _canvasGroup.DOFade(_closePreset.endAlpha, _closePreset.duration)
                    .SetEase(_closePreset.easeType)
            );
        }

        if (_rectTransform != null)
        {
            _currentAnimation.Join(
                _rectTransform.DOScale(_closePreset.endScale, _closePreset.duration)
                    .SetEase(_closePreset.easeType)
            );
        }

        _currentAnimation.Play();

        while (_currentAnimation != null && _currentAnimation.IsActive() && _currentAnimation.IsPlaying())
        {
            yield return null;
        }

        gameObject.SetActive(false);
        onComplete?.Invoke();
        _currentCoroutine = null;
    }

    private void CancelCurrentAnimation()
    {
        if (this == null || gameObject == null) return;
        if (_currentAnimation == null) return;
        if (_currentAnimation.IsActive())
        {
            _currentAnimation.Kill();
            _currentAnimation = null;
        }
    }

    private void CancelCurrentCoroutine()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
            _currentCoroutine = null;
        }
    }

    private void OnEnable()
    {
        CancelCurrentAnimation();
        CancelCurrentCoroutine();
    }

    private void OnDisable()
    {
        CancelCurrentAnimation();
        CancelCurrentCoroutine();
    }

    public void PlayOpenAnimation()
    {
        PlayOpenAnimationCoroutine();
    }

    public void PlayCloseAnimation()
    {
        PlayCloseAnimationCoroutine();
    }

    public static UIAnimationComponent UsingAnimation(GameObject gameObject) =>
        gameObject.TryGetComponent(out UIAnimationComponent component)
            ? component
            : gameObject.AddComponent<UIAnimationComponent>();

    public UIAnimationComponent ApplyCanvasGroup(CanvasGroup canvasGroup)
    {
        _canvasGroup = canvasGroup;
        return this;
    }

    public UIAnimationComponent ApplyRectTransform(RectTransform rectTransform)
    {
        _rectTransform = rectTransform;
        return this;
    }

    public UIAnimationComponent ApplyPreset(UIAnimationPreset preset)
    {
        _openPreset = preset;
        _closePreset = preset;
        return this;
    }

    public UIAnimationComponent ApplyPresetOpen(UIAnimationPreset preset)
    {
        _openPreset = preset;
        return this;
    }

    public UIAnimationComponent ApplyPresetClose(UIAnimationPreset preset)
    {
        _closePreset = preset;
        return this;
    }
}
