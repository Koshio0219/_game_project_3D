using Cysharp.Threading.Tasks;
using Game.Hud;
using Game.Player;
using System;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "AfterimageDodgeSoul", menuName = "SwordSoul/Dodge/Afterimage")]
    public class AfterimageDodgeSoul : SwordSoul
    {
        public Material afterimageMaterial;
        public float duration = 0.3f;
        public float addHitRate = 0.5f;
        public float addCritRate = 0.5f;

        public AfterimageDodgeSoul()
        {
            soulID = "幻影冲锋";
            triggerType = SoulTriggerType.Dodge;
            description = "命中率，暴击率+50%，留下残像迷惑敌人";
        }

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] AfterimageDodge activated!");
            var pm = PlayerPropManager.Instance.Prop;
            if (pm == null) return;

            pm.HitRate += addHitRate;
            pm.CritRate += addCritRate;
            pm.InvokeChanged();

            // 创建残像
            var clone = GameObject.Instantiate(player);
            //GameObject.Destroy(clone.GetComponent<Collider>());
            //GameObject.Destroy(clone.GetComponent<Rigidbody>());

            var renderers = clone.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.material = afterimageMaterial;

            GameObject.Destroy(clone, duration);

            // 简单演出
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            UIMessageSystem.Instance.AddMessage($"触发闪避剑魂:{soulID}");
        }
    }
}
