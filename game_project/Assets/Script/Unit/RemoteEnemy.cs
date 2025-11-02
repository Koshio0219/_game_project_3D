using Cysharp.Threading.Tasks;
using Game.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Game.Unit
{
    public class RemoteEnemy : Enemy
    {
        public List<FireBase> normal_fire;
        public List<FireBase> skill_fire;

        public override void Attack(int targetId, float damage)
        {
            if (test_skill_fire) return;

            normal_fire.ForEach(one =>
            {
                Fire(one, targetId, damage);
                //SEManager.Instance.Play(SEPath.ENEMY_BULLET_ATTACK, volumeRate: 0.4f);
            });
        }

        private void Fire(FireBase fire,int targetId, float damage)
        {
            if (fire == null) return;
            var target = GameManager.stageManager.GetPlayer(targetId);
            var creatorId = EnemyUnitData.InsId;

            fire.SetTarget(target).
                SetFirePos(fire.transform).
                SetDamage(damage + Atk).
                FireBegin(creatorId);
            base.Attack(targetId, damage);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            normal_fire.ForEach(one => one.FireShut());
            skill_fire.ForEach(one => one.FireShut());
        }

        //test skill_fire
        public bool test_skill_fire = false;
        private void Start()
        {
            if (!test_skill_fire) return;
            UniTask.Void(async () =>
            {
                while(this && isActiveAndEnabled)
                {
                    skill_fire.ForEach(one =>
                    {
                        Fire(one, 0, 50);
                    });
                    await UniTask.Delay(500);
                }
            });
        }
    }
}

