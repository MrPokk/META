using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class DissolveFullScreen : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material _dissolveMaterial;
    [SerializeField, Range(0, 5)] private float _duration = 2.0f;

    private readonly string _shaderPropertyName = "_DissolveAmount";
    private CancellationTokenSource _cts;

    public float DissolveAmount
    {
        get => _dissolveMaterial.GetFloat(_shaderPropertyName);
        set
        {
            if (_dissolveMaterial == null)
            {
                return;
            }

            if (value > 1)
            {
                value = 1;
            }

            _dissolveMaterial.SetFloat(_shaderPropertyName, value);
        }
    }

    public async UniTask StartDissolve(float targetValue)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        await AnimateDissolveAsync(targetValue, _cts.Token);
    }

    private async UniTask AnimateDissolveAsync(float target, CancellationToken token)
    {
        if (_dissolveMaterial == null)
        {
            return;
        }

        var startValue = _dissolveMaterial.GetFloat(_shaderPropertyName);
        float elapsed = 0;

        while (elapsed < _duration)
        {
            token.ThrowIfCancellationRequested();

            elapsed += Time.deltaTime;
            var normalizedTime = elapsed / _duration;

            var newValue = Mathf.Lerp(startValue, target, normalizedTime);
            _dissolveMaterial.SetFloat(_shaderPropertyName, newValue);

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        _dissolveMaterial.SetFloat(_shaderPropertyName, target);
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            StartDissolve(1f).Forget();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartDissolve(0f).Forget();
        }
    }
#endif

    private void OnDestroy()
    {
        _dissolveMaterial.SetFloat(_shaderPropertyName, 0f);

        _cts?.Cancel();
        _cts?.Dispose();
    }
}
