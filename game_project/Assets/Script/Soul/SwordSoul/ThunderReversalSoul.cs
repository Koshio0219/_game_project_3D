using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

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
                if (hit.CompareTag("Enemy"))
                {
                    Debug.Log($"Enemy {hit.name} takes {damage} lightning damage!");
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }
    }
}
