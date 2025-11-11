using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using Game.Hud;
using System;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "ThunderReversalSoul", menuName = "SwordSoul/Parry/ThunderReversal")]
    public class ThunderReversalSoul : SwordSoul
    {
        public float lightningRadius = 5f;
        public float damage = 50f;

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] ThunderReversal activated!");

            //特效
            EffectManager.Instance.PlayEffect("Range", player.transform.position + Vector3.up,1);

            // 示例敌人伤害逻辑
            var hits = Physics.OverlapSphere(player.transform.position, lightningRadius);
            foreach (var hit in hits)
            {
                var root = hit.transform.root;
                if (root.TryGetComponent<IEnemyBaseAction>(out var enemy))
                {
                    EventQueueSystem.QueueEvent(new SendDamageEvent(player.transform.root.GetInstanceID(), enemy.EnemyUnitData.InsId, damage));
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            UIMessageSystem.Instance.AddMessage($"触发招架剑魂:{soulID}");
        }
    }
}
