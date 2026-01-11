using UnityEngine;

public class PlayerModelComponent : MonoBehaviour
{
    private string _isIdle = "IsIdle";
    private string _isWalk = "IsWalk";

   private PlayerProvider _playerProvider;

    public Animator Animator { get; private set; }


    private void Awake()
    {
        Animator ??= GetComponentInChildren<Animator>();
        _playerProvider ??= GetComponentInParent<PlayerProvider>();
    }

    public void Hidden() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
    public void Toggle() => gameObject.SetActive(!gameObject.activeSelf);
    public void SetView(bool isView) => gameObject.SetActive(isView);

    public void SetIdle() => Animator.SetTrigger(_isIdle);
    public void SetWalk() => Animator.SetTrigger(_isWalk);
    public void SetAnimationState(StateComponent.State state)
    {
        switch (state)
        {
            case StateComponent.State.Idle:
                Animator.SetTrigger(_isIdle);
                break;
            case StateComponent.State.Moving:
                Animator.SetTrigger(_isWalk);
                break;
            default:
                break;
        }
    }

    public void SetSpeedAnimation()
    {
        if (_playerProvider == null)
        {
            return;
        }

        if (Animator == null)
        {
            return;
        }

        var animationSpeedMultiplier = 1;
        var speedPlayer = _playerProvider.CharacterController.velocity.magnitude;
        Animator.speed = speedPlayer > 0.1f ? speedPlayer * animationSpeedMultiplier : 1;
    }
}
