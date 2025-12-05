using System.Collections.Generic;
using BitterECS.Core;
using UnityEngine;

public class PlayerAnimationSystem : IPlayerUsingSystem
{
    public Priority PrioritySystem => Priority.Medium;

    private string _isWalk = "IsWalk";
    private string _isRun = "IsRun";


    public void OnRun(PlayerProvider player)
    {
        if (player.Animator == null)
        {
            Debug.LogError("Animator is null!");
            return;
        }

        if (player.Animator.runtimeAnimatorController == null)
        {
            Debug.LogError("AnimatorController is not assigned!");
            return;
        }

        var isMove = player.CharacterController.velocity.magnitude > 0.1f;
        player.Animator.SetBool(_isWalk, isMove);
    }
}
