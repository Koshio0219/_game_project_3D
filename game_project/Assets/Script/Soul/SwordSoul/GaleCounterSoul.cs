using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "GaleCounterSoul", menuName = "SwordSoul/Parry/GaleCounter")]
    public class GaleCounterSoul : SwordSoul
    {
        public float shockwaveRadius = 4f;
        public float knockbackForce = 6f;

        public GaleCounterSoul()
        {
            soulID = "Parry_GaleCounter";
            triggerType = SoulTriggerType.Parry;
            description = "成功招架时释放气浪，将周围敌人击退。";
        }

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] GaleCounter activated!");

            // 例：生成一个简易气浪特效
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = player.transform.position;
            effect.transform.localScale = Vector3.one * shockwaveRadius;
            effect.GetComponent<Renderer>().material.color = Color.cyan;
            GameObject.Destroy(effect, 0.4f);

            // 敌人击退逻辑
            var colliders = Physics.OverlapSphere(player.transform.position, shockwaveRadius);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    Vector3 dir = (col.transform.position - player.transform.position).normalized;
                    col.attachedRigidbody?.AddForce(dir * knockbackForce, ForceMode.Impulse);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }
    }
}
