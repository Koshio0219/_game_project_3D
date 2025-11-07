using Game.Player;
using UnityEngine;

public class IdleStateSync : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var stateHandler = animator.GetComponentInParent<PlayerStateHandler>();
        if (stateHandler != null && stateHandler.State != PlayerAnimatorState.Idle)
        {
            stateHandler.State = PlayerAnimatorState.Idle;
        }
    }
}
