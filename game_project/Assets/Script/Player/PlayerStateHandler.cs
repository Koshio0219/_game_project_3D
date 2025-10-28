using System.Collections;
using UnityEngine;

namespace Game.Player
{
    public enum PlayerAnimatorState
    {
        Idle,
        Running,
        Jump,
    }

    public class PlayerStateHandler : MonoBehaviour
    {
        public Animator animator;
        private PlayerAnimatorState state = PlayerAnimatorState.Idle;
        public PlayerAnimatorState State
        {
            get => state;
            set => OnPlayerStateChange(value);
        }

        private void OnPlayerStateChange(PlayerAnimatorState to)
        {
            if (to == state)
                return;

            switch (to)
            {
                case PlayerAnimatorState.Idle:
                    {
                        animator.CrossFade("Idle", .2f);
                        //animator.speed = 1f;
                        break;
                    }
                case PlayerAnimatorState.Running:
                    {
                        animator.CrossFade("Running", .2f);
                        //animator.speed = 1f;
                        break;
                    }
                case PlayerAnimatorState.Jump:
                    {
                        animator.CrossFade("Jump", .2f);
                        //animator.speed = 1.5f;
                        break;
                    }
            }

            state = to;
        }
    }
}