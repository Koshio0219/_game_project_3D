using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Game.Soul
{
    [CreateAssetMenu(fileName = "TimeRiftSoul", menuName = "SwordSoul/Dodge/TimeRift")]
    public class TimeRiftSoul : SwordSoul
    {
        public float slowTimeScale = 0.2f;
        public float duration = 0.8f;

        public override async UniTask ApplyEffectAsync(GameObject player, GameObject attacker = null)
        {
            Debug.Log("[SwordSoul] TimeRift activated!");
            Time.timeScale = slowTimeScale;
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
            Time.timeScale = 1f;
        }
    }
}
