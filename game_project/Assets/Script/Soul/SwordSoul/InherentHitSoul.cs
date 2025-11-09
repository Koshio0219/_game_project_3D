using Cysharp.Threading.Tasks;
using Game.Framework;
using Game.Player;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "Inherent_Hit_Soul", menuName = "Game/Soul/InherentHitSoul")]
    public class InherentHitSoul : SwordSoul
    {
        public float addHitRate = .5f;

        public override async UniTask ApplyEffectAsync(GameObject context = null, GameObject context2 = null)
        {
            var pm = PlayerPropManager.Instance;
            if (pm != null)
            {
                pm.Prop.HitRate += addHitRate;
                pm.Prop.InvokeChanged();
            }

            // 简单演出：等待 300ms（可以改成播放粒子或UI）
            await UniTask.Delay(300);
        }
    }
}
