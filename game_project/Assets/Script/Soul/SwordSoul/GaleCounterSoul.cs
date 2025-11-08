using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using Game.Player;
using System;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "GaleCounterSoul", menuName = "SwordSoul/Parry/GaleCounter")]
    public class GaleCounterSoul : SwordSoul
    {
        public float shockwaveRadius = 4f;
        public float knockbackForce = 6f;

        public GaleCounterSoul()
        {
            soulID = "烈风反击";
            triggerType = SoulTriggerType.Parry;
            description = "消耗所有剑气，对周围敌人造成：消耗剑气数x100点伤害";
        }

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] GaleCounter activated!");

            var pm = PlayerPropManager.Instance.Prop;
            if (pm == null) return;

            //生成一个简易气浪特效
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.transform.position = player.transform.position;
            effect.transform.localScale = Vector3.one * shockwaveRadius;
            effect.GetComponent<Renderer>().material.color = Color.cyan;
            Destroy(effect, 0.4f);

            // 敌人击退逻辑
            int swordPoint = pm.SwordPoint;
            pm.SwordPoint = 0;
            var colliders = Physics.OverlapSphere(player.transform.position, shockwaveRadius);
            foreach (var col in colliders)
            {
                var root = col.transform.root;
                if (root.TryGetComponent<IEnemyBaseAction>(out var enemy))
                {
                    EventQueueSystem.QueueEvent(new SendDamageEvent(player.transform.root.GetInstanceID(), enemy.EnemyUnitData.InsId, swordPoint*100));
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }
    }
}
