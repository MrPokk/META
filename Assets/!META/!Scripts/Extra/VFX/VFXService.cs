using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(DissolveFullScreen))]
public class VFXService : MonoBehaviour
{
    private static VFXService _instance;
    public static VFXService Instance => _instance;

    private DissolveFullScreen _dissolveFullScreen;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        _dissolveFullScreen = GetComponent<DissolveFullScreen>();
    }

    public static void OnClientSceneTransitionSet(float value) => Instance._dissolveFullScreen.DissolveAmount = value;

    public static void OnClientSceneTransitionStart(Action  onComplete = null) => Instance._dissolveFullScreen.StartDissolve(1f, onComplete).Forget();
    public static void OnClientSceneTransitionComplete(Action onComplete = null) => Instance._dissolveFullScreen.StartDissolve(0f, onComplete).Forget();
}
