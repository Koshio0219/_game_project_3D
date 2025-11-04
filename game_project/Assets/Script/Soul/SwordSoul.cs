using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Game.Soul
{
    public enum SoulTriggerType { Inherent, Dodge, Parry }

    public abstract class SwordSoul : ScriptableObject
    {
        [Header("Basic")]
        public string soulID;
        public SoulTriggerType triggerType = SoulTriggerType.Dodge;
        [TextArea(2, 5)]
        public string description;
        public Sprite icon;

        // 是否在本关已被触发（由 Manager 管理）
        [HideInInspector]
        public bool usedInStage = false;

        /// <summary>
        /// 执行剑魂效果（异步），context 可传入触发来源（Player/Enemy 等）
        /// 返回值：完成时机（用于 UI 演出等待）
        /// </summary>
        public abstract UniTask ApplyEffectAsync(GameObject context1 = null, GameObject context2 = null);
    }
}
