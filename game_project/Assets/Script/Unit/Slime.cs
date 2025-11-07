using Game.Data;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Game.Unit
{
    public class Slime : Enemy
    {
        public Face faces;
        public GameObject smileBody;
        public int damType;

        private Material faceMaterial;

        public override void Born(EnemyUnitData data)
        {
            base.Born(data);
            faceMaterial = smileBody.GetComponent<Renderer>().materials[1];
        }

        protected override void InitBehaviorTree(List<Transform> list)
        {
            base.InitBehaviorTree(list);
        }

        void SetFace(Texture tex)
        {
            faceMaterial.SetTexture("_MainTex", tex);
        }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        /// <param name="speed"> 0为静止Idle；>0 为Walk；</param>
        void SetSpeed(float speed)
        {
            animator.SetFloat("Speed", speed);
        }

        protected override void OnChangeIdle()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return;
            SetFace(faces.Idleface);
            SetSpeed(1.2f);
        }

        protected override void OnChangeMove()
        {
            SetFace(faces.WalkFace);
            SetSpeed(1.2f);
        }

        protected override void OnChangeAttack()
        {
            switch (EnemyAttackState)
            {
                case AttackState.Normal:
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) break;
                    SetFace(faces.attackFace);
                    animator.SetTrigger("Attack");
                    //SetSpeed();
                    break;
                case AttackState.Skill:
                    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Jump")) return;
                    SetFace(faces.jumpFace);
                    animator.SetTrigger("Jump");
                    //SetSpeed();
                    break;
            }
        }

        protected override void OnChangeHit()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Damage0")
                || animator.GetCurrentAnimatorStateInfo(0).IsName("Damage1")
                || animator.GetCurrentAnimatorStateInfo(0).IsName("Damage2")) return;

            animator.SetTrigger("Damage");
            animator.SetInteger("DamageType", damType);
            SetFace(faces.damageFace);
           // SetSpeed();
        }
    }
}