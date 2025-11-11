using Cysharp.Threading.Tasks;
using Game.Framework;
using Game.Hud;
using Game.Player;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "Inherent_HP_Soul", menuName = "Game/Soul/InherentHPSoul")]
    public class InherentHPSoul : SwordSoul
    {
        public int addMaxHP = 100;

        public override async UniTask ApplyEffectAsync(GameObject context=null, GameObject context2 = null)
        {
            var pm = PlayerPropManager.Instance;
            if (pm != null)
            {
                pm.Prop.MaxHP += addMaxHP;
                EventQueueSystem.QueueEvent(new PlayerHpChangeEvent(pm.Prop.HP, pm.Prop.HP, pm.Prop.MaxHP));
            }

            // 简单演出：等待 300ms（可以改成播放粒子或UI）
            await UniTask.Delay(300);
            UIMessageSystem.Instance.AddMessage($"触发固有剑魂:{soulID}");
        }
    }
}
