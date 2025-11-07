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
        Skill,
        Hurt, //受伤
        Dodge, //闪避
        ParrySuccess, //招架成功
        Parry, //招架
        Dead
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
            if (animator == null)
                return;

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
                        animator.CrossFade($"Attack1", .2f,1);
                        break;
                    }
                case PlayerAnimatorState.Skill:
                    {
                        animator.CrossFade($"Attack2", .2f, 1);
                        break;
                    }
                case PlayerAnimatorState.Hurt:
                    {
                        //短动画直接播放不CrossFade
                        animator.Play("Hurt");
                        break;
                    }
                case PlayerAnimatorState.Dodge:
                    {
                        animator.CrossFade("Dodge", .2f);
                        break;
                    }
                case PlayerAnimatorState.ParrySuccess:
                    {
                        animator.Play("Parry");
                        break;
                    }
                case PlayerAnimatorState.Parry:
                    {
                        animator.Play("Parry");
                        break;
                    }
                case PlayerAnimatorState.Dead:
                    {
                        animator.CrossFade("Dead", .2f);
                        break;
                    }
            }

            state = to;
        }

        public TimerTask idleDanceTask;

        private void Awake()
        {
            if (idleDanceTask == null || animator == null)
                return;
            idleDanceTask.OnTimerComplete += SwitchIdle;
        }

        private void OnDestroy()
        {
            if (idleDanceTask == null || animator == null)
                return;
            idleDanceTask.OnTimerComplete -= SwitchIdle;
        }

        private void SwitchIdle()
        {
            if (animator == null)
                return;
            var isDance = animator.GetBool("IsIdleDance");
            if (isDance)
                animator.SetBool("IsIdleDance", false);
            else
                animator.SetBool("IsIdleDance", true);
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