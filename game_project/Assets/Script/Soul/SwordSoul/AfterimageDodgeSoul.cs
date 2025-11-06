using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "AfterimageDodgeSoul", menuName = "SwordSoul/Dodge/Afterimage")]
    public class AfterimageDodgeSoul : SwordSoul
    {
        public Material afterimageMaterial;
        public float duration = 0.3f;

        public AfterimageDodgeSoul()
        {
            soulID = "Dodge_Afterimage";
            triggerType = SoulTriggerType.Dodge;
            description = "极限闪避成功时，留下残像迷惑敌人。";
        }

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] AfterimageDodge activated!");

            // 创建残像
            var clone = GameObject.Instantiate(player);
            GameObject.Destroy(clone.GetComponent<Collider>());
            GameObject.Destroy(clone.GetComponent<Rigidbody>());

            var renderers = clone.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.material = afterimageMaterial;

            GameObject.Destroy(clone, duration);
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }
    }
}
