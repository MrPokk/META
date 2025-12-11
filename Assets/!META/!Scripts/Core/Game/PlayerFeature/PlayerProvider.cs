using BitterECS.Integration;
using Mirror;
using UnityEngine;
using static StateComponent;

[RequireComponent(typeof(MovingComponentProvider))]
[RequireComponent(typeof(CharacterController))]
public class PlayerProvider : MonoProvider<PlayerPresenter>, ITeleported, IUsingQuestions
{
    public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public CameraObjectComponent CameraObjectComponent { get; private set; }
    public Animator animator;

    private NetworkIdentity _networkIdentity;

    protected override void Registration()
    {
        Entity.Add<StateComponent>(new(State.Idle));

        CharacterController = GetComponent<CharacterController>();
        CameraObjectComponent ??= GetComponentInChildren<CameraObjectComponent>();
        _networkIdentity ??= GetComponent<NetworkIdentity>();
        animator ??= GetComponent<Animator>();
    }

    private void Start()
    {
        DeleteComponent();
    }

    private void DeleteComponent()
    {
        if (Entity.Has<ControllableComponent>())
        {
            return;
        }

        Destroy(CameraObjectComponent.gameObject);
    }

    public void EnterQuestion(QuestionPoint questionPoint)
    {
        if (Entity.Has<ControllableComponent>())
        {
            UIRootManager.OpenPopup<UIQuestionPopup>();
        }
    }

    public void ExitQuestion(QuestionPoint questionPoint)
    {
        if (Entity.Has<ControllableComponent>())
        {
            UIRootManager.ClosePopup<UIQuestionPopup>();
        }
    }

    public void EnterTeleport(TeleportPoint teleportPoint)
    {
        if (Entity.Has<ControllableComponent>())
        {
            UIRootManager.OpenPopup<UITeleportPopup>();
        }
    }

    public void ExitTeleport(TeleportPoint teleportPoint)
    {
        if (Entity.Has<ControllableComponent>())
        {
            UIRootManager.ClosePopup<UITeleportPopup>();
        }
    }
}

