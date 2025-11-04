using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Game.Framework;
using UnityEngine.Events;

namespace Game.Soul
{
    public class SwordSoulManager : MonoSingleton<SwordSoulManager>
    {
        [Header("All Souls (Repository) - optional")]
        public List<SwordSoul> allSouls = new();

        [Header("Current Deck (in-order)")]
        public List<SwordSoul> currentDeck = new();

        // 当前关已经触发的 souls（不再于本关重复触发）
        private readonly HashSet<string> usedSoulIDs = new();

        // 事件：当当前Deck变化（用于UI刷新）
        public event UnityAction OnDeckChanged;

        //private void Start()
        //{
        //    // 自动触发固有剑魂
        //    _ = ApplyInherentSoulsAsync();
        //}

        public void SetDeckOrder(List<SwordSoul> newOrder)
        {
            currentDeck = new List<SwordSoul>(newOrder);
            OnDeckChanged?.Invoke();
        }

        public void MoveSoul(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= currentDeck.Count) return;
            if (toIndex < 0 || toIndex > currentDeck.Count) return;

            var item = currentDeck[fromIndex];
            currentDeck.RemoveAt(fromIndex);
            if (toIndex >= currentDeck.Count) currentDeck.Add(item);
            else currentDeck.Insert(toIndex, item);

            OnDeckChanged?.Invoke();
        }

        public SwordSoul GetNextUnusedSoul(SoulTriggerType type)
        {
            return currentDeck.FirstOrDefault(s => s.triggerType == type && !usedSoulIDs.Contains(s.soulID));
        }

        public async UniTask ApplyInherentSoulsAsync()
        {
            // 在关卡开始时触发所有类型为 Inherent 的剑魂（按队列顺序）
            foreach (var s in currentDeck)
            {
                if (s.triggerType != SoulTriggerType.Inherent) continue;
                if (usedSoulIDs.Contains(s.soulID)) continue;

                usedSoulIDs.Add(s.soulID);
                await s.ApplyEffectAsync();
            }

            OnDeckChanged?.Invoke();
        }

        /// <summary>
        /// 在闪避时调用（由 Player 控制器在检测完perfect dodge后触发）
        /// 按队列顺序选取下一个 type == Dodge 且 未使用 的剑魂触发
        /// </summary>
        public async UniTask TriggerOnDodgeAsync(GameObject ownerContext = null)
        {
            var s = GetNextUnusedSoul(SoulTriggerType.Dodge);
            if (s == null) return;

            usedSoulIDs.Add(s.soulID);
            OnDeckChanged?.Invoke();

            // 短暂时停演出（UI可播放），推荐用 UniTask.Delay
            await s.ApplyEffectAsync(ownerContext);
        }

        /// <summary>
        /// 在招架时触发
        /// </summary>
        public async UniTask TriggerOnParryAsync(GameObject attackerContext,GameObject enemyContext = null)
        {
            var s = GetNextUnusedSoul(SoulTriggerType.Parry);
            if (s == null) return;

            usedSoulIDs.Add(s.soulID);
            OnDeckChanged?.Invoke();

            await s.ApplyEffectAsync(attackerContext, enemyContext);
        }

        /// <summary>
        /// 关卡结束时，允许按玩家意愿重组（示例：把 used 的重新放回到队列末尾或任意位置）
        /// newOrder 必须包含和 currentDeck 相同的元素（引用）
        /// </summary>
        public void RebuildDeck(List<SwordSoul> newOrder, bool resetUsed = false)
        {
            // 你可以在这里校验 newOrder，示例简单替换
            SetDeckOrder(newOrder);
            if (resetUsed) usedSoulIDs.Clear();
            OnDeckChanged?.Invoke();
        }

        /// <summary>
        /// 测试：清空已使用状态（用于Debug）
        /// </summary>
        public void ResetUsed()
        {
            usedSoulIDs.Clear();
            foreach (var s in currentDeck) s.usedInStage = false;
            OnDeckChanged?.Invoke();
        }

        public bool IsUsed(SwordSoul soul) => usedSoulIDs.Contains(soul.soulID);
    }
}
