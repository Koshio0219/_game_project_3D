using Game.Base;
using System.Collections;
using UnityEngine;

namespace Game.Player
{
    public enum PlayerAnimatorState
    {
        Idle,
        Running,
        Jump,
        Attack,
        Hurt
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

        private float duration;

        private void OnPlayerStateChange(PlayerAnimatorState to)
        {
            if (to == state)
                return;

            //if (!CheckStateConfig(to))
            //    return;

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
                case PlayerAnimatorState.Attack:
                    {
                        var idx = Random.Range(1, 3);
                        Debug.Log($"Attack{idx}");
                        animator.CrossFade($"Attack{idx}", .2f,1);
                        
                        //animator.speed = 1f;
                        break;
                    }
            }

            state = to;
        }

        private bool CheckStateConfig(PlayerAnimatorState to)
        {
            var data = GameManager.Instance.gameData.playerStateConfigs;
            //优先级高的状态可以覆盖优先级低的状态
            if (data[state].priority <= data[to].priority) return true;
            //如果状态持续时间小于等于当前状态持续时间，则可以切换
            if (duration > data[state].duration)
            {
                duration = 0;
                return true;
            }
            return false;
        }

        //void Update()
        //{
        //    if (duration < 20)
        //        duration += Time.deltaTime;
        //}
    }
}