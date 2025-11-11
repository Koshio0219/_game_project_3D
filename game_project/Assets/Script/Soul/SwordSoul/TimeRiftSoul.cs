using Cysharp.Threading.Tasks;
using Game.Hud;
using Game.Player;
using System;
using UnityEngine;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "TimeRiftSoul", menuName = "SwordSoul/Dodge/TimeRift")]
    public class TimeRiftSoul : SwordSoul
    {
        public float slowTimeScale = 0.2f;
        public float duration = 2f;
        public int addSwordPoint = 3;
        public int addMaxSwordPoint = 1;

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] TimeRift activated!");
            var pm = PlayerPropManager.Instance.Prop;
            if (pm == null) return;
            pm.MaxSwordPoint += addMaxSwordPoint;
            pm.SwordPoint += addSwordPoint;
            UIMessageSystem.Instance.AddMessage($"触发闪避剑魂:{soulID}");
            Time.timeScale = slowTimeScale;
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            Time.timeScale = 1f;
        }
    }
}
