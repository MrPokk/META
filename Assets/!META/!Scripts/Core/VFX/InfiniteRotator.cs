using UnityEngine;
using DG.Tweening;

[DisallowMultipleComponent]
public class InfiniteRotator : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float _duration = 2f;
    [SerializeField] private Ease _easeType = Ease.Linear;

    private Tween _rotationTween;
    private Transform _cachedTransform;

    private void Awake()
    {
        _cachedTransform = transform;
    }

    private void OnEnable()
    {
        StartRotation();
    }

    private void OnDisable()
    {
        StopRotation();
    }

    private void OnDestroy()
    {
        StopRotation();
    }

    private void StartRotation()
    {
        StopRotation();

        _rotationTween = _cachedTransform.DORotate(new Vector3(0, 360, 0), _duration, RotateMode.LocalAxisAdd)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Restart)
            .SetRecyclable(true)
            .Play();
    }

    private void StopRotation()
    {
        if (_rotationTween != null && _rotationTween.IsActive())
        {
            _rotationTween.Kill();
        }
        _rotationTween = null;
    }
}
