using UnityEngine;

[RequireComponent(typeof(DissolveFullScreen))]
public class VFXService : MonoBehaviour
{
    private DissolveFullScreen _dissolveFullScreen;

    public DissolveFullScreen DissolveFullScreen { get => _dissolveFullScreen; }

    private void Awake()
    {
        _dissolveFullScreen = GetComponent<DissolveFullScreen>();
    }
}
