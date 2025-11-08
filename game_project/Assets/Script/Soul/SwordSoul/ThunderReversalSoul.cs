using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
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

            // 生成特效
            var fx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fx.transform.position = player.transform.position;
            fx.transform.localScale = Vector3.one * lightningRadius;
            fx.GetComponent<Renderer>().material.color = Color.yellow;
            GameObject.Destroy(fx, 0.3f);

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
        }
    }
}
