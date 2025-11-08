using Game.Data;
using Game.Framework;
using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Unit
{
    public class FlyGhost : RemoteEnemy
    {
        public GameObject ghost_normal;
        public GameObject ghost_parts;
        public float animationCrossTime = .5f;

        public override void Born(EnemyUnitData data)
        {
            base.Born(data);
            ghost_normal.Show();
            ghost_parts.Hide();
            transform.rotation = Quaternion.identity;
        }

        protected override void OnChangeAttack()
        {
            Play("attack");
            SEManager.Instance.Play(SEPath.ENEMY_GHOST_ATTACK, volumeRate: 0.7f);
        }

        protected override void OnChangeDead()
        {
            ghost_normal.Hide();
            ghost_parts.Show();
        }

        protected override void OnChangeMove()
        {
            OnChangeIdle();
        }

        protected override void OnChangeIdle()
        {
            Play("idle");
            //animator.Play("idle");
        }

        private void Play(string name)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsName(name)) return;
            animator.CrossFade(name, animationCrossTime);
        }
    }
}

