using BitterECS.Integration;
using UnityEngine;

[RequireComponent(typeof(MovingComponentProvider))]
public class PlayerProvider : MonoProvider<PlayerPresenter>, ITeleported, IUsingQuestions
{
    public CharacterController CharacterController { get; private set; }

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
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
