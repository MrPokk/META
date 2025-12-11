using BitterECS.Integration;
using UnityEngine;
using static StateComponent;

[RequireComponent(typeof(MovingComponentProvider))]
[RequireComponent(typeof(CharacterController))]
public class PlayerProvider : MonoProvider<PlayerPresenter>, ITeleported, IUsingQuestions
{
    public CharacterController CharacterController { get; private set; }
    public Animator animator;

    protected override void Registration()
    {
        Entity.Add<StateComponent>(new(State.Idle));

        CharacterController = GetComponent<CharacterController>();
        animator ??= GetComponent<Animator>();
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

