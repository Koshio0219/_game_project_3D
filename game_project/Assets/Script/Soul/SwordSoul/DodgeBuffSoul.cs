using UnityEngine;
using Cysharp.Threading.Tasks;
using Game.Player;
using System;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "Dodge_Buff_Soul", menuName = "Game/Soul/DodgeBuffSoul")]
    public class DodgeBuffSoul : SwordSoul
    {
        public float buffAtk = 50f;
        public float durationSec = 8f;

        public override async UniTask ApplyEffectAsync(GameObject context1 = null, GameObject context2 = null)
        {
            var pm = PlayerPropManager.Instance.Prop;
            if (pm == null) return;

            pm.AtkPoint += buffAtk;
            pm.InvokeChanged();

            // 简单演出
            await UniTask.Delay(200);

            // buff 持续期间（不阻塞主流程），使用独立任务在后台回退
            _ = UniTask.Delay(TimeSpan.FromSeconds(durationSec)).ContinueWith(() =>
            {
                pm.AtkPoint -= buffAtk;
                pm.InvokeChanged();
            });

            // 立刻完成触发（UI可等待短暂演出）
        }
    }
}
