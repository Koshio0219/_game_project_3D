using Cysharp.Threading.Tasks;
using Game.Hud;
using Game.Player;
using System;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "Dodge_Buff_Soul", menuName = "Game/Soul/DodgeBuffSoul")]
    public class DodgeBuffSoul : SwordSoul
    {
        public float buffAtk = 80f;
        public float durationSec = 8f;
        public float buffHitRate = 0.8f;
        public float buffCritDamageRate = 0.8f;

        public DodgeBuffSoul() {
            soulID = "敏捷之魂";
            description = "8秒内攻击力+80，命中率和暴击伤害+80%";
            triggerType = SoulTriggerType.Dodge;
        }

        public override async UniTask ApplyEffectAsync(GameObject context1 = null, GameObject context2 = null)
        {
            var pm = PlayerPropManager.Instance.Prop;
            if (pm == null) return;

            pm.AtkPoint += buffAtk;

            //可能超过100%
            float lastHitRate = pm.HitRate;
            pm.HitRate += buffHitRate;
            float addHitRate = pm.HitRate - lastHitRate;

            pm.CritDmg += buffCritDamageRate;
            pm.InvokeChanged();

            // 简单演出
            await UniTask.Delay(200);
            UIMessageSystem.Instance.AddMessage($"触发闪避剑魂:{soulID}");

            // buff 持续期间（不阻塞主流程），使用独立任务在后台回退
            _ = UniTask.Delay(TimeSpan.FromSeconds(durationSec)).ContinueWith(() =>
            {
                pm.AtkPoint -= buffAtk;
                pm.HitRate-= addHitRate;
                pm.CritDmg -= buffCritDamageRate;
                UIMessageSystem.Instance.AddMessage($"闪避剑魂:{soulID}效果结束");
                pm.InvokeChanged();
            });

            // 立刻完成触发（UI可等待短暂演出）
        }
    }
}
