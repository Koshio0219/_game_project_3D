using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using Game.Framework;

namespace Game.Soul
{
    public class SwordSoulManager : MonoSingleton<SwordSoulManager>
    {
        [Header("All Souls (Repository)")]
        public List<SwordSoul> allSouls = new();

        [Header("Current Deck (in-order)")]
        public List<SwordSoul> currentDeck = new();

        // 分类缓存：按类型快速取出同类剑魂
        private readonly Dictionary<SoulTriggerType, List<SwordSoul>> categorizedDeck = new();

        // 已使用剑魂 ID
        private readonly HashSet<string> usedSoulIDs = new();

        public event UnityAction OnDeckChanged;

        #region === Core Deck Logic ===

        private void RebuildCategoryCache()
        {
            categorizedDeck.Clear();
            foreach (SoulTriggerType type in System.Enum.GetValues(typeof(SoulTriggerType)))
                categorizedDeck[type] = new List<SwordSoul>();

            foreach (var s in currentDeck)
                categorizedDeck[s.triggerType].Add(s);
        }

        public void SetDeckOrder(List<SwordSoul> newOrder, bool invokeEvent = true)
        {
            if (newOrder == null || newOrder.Count == 0)
                return;

            currentDeck = new List<SwordSoul>(newOrder);
            RebuildCategoryCache();

            if (invokeEvent)
                OnDeckChanged?.Invoke();
        }

        public void MoveSoul(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= currentDeck.Count) return;
            if (toIndex < 0 || toIndex >= currentDeck.Count) return;

            var item = currentDeck[fromIndex];
            currentDeck.RemoveAt(fromIndex);
            currentDeck.Insert(toIndex, item);

            // 更新分类缓存
            RebuildCategoryCache();

            OnDeckChanged?.Invoke();
        }

        public void RebuildDeck(List<SwordSoul> newOrder, bool resetUsed = false)
        {
            SetDeckOrder(newOrder, invokeEvent: false);

            if (resetUsed)
                usedSoulIDs.Clear();

            OnDeckChanged?.Invoke(); // 仅触发一次
        }

        #endregion

        #region === Soul Selection ===

        public SwordSoul GetNextUnusedSoul(SoulTriggerType type)
        {
            if (!categorizedDeck.ContainsKey(type)) return null;

            return categorizedDeck[type]
                .FirstOrDefault(s => !usedSoulIDs.Contains(s.soulID));
        }

        public bool IsUsed(SwordSoul soul) => usedSoulIDs.Contains(soul.soulID);

        public void ResetUsed()
        {
            usedSoulIDs.Clear();
            foreach (var s in currentDeck)
                s.usedInStage = false;

            OnDeckChanged?.Invoke();
        }

        #endregion

        #region === Triggers ===

        public async UniTask ApplyInherentSoulsAsync()
        {
            if (!categorizedDeck.ContainsKey(SoulTriggerType.Inherent))
                return;

            foreach (var s in categorizedDeck[SoulTriggerType.Inherent])
            {
                if (usedSoulIDs.Contains(s.soulID)) continue;
                usedSoulIDs.Add(s.soulID);
                await s.ApplyEffectAsync();
            }

            OnDeckChanged?.Invoke();
        }

        public async UniTask TriggerOnDodgeAsync(GameObject ownerContext = null)
        {
            var s = GetNextUnusedSoul(SoulTriggerType.Dodge);
            if (s == null) return;

            usedSoulIDs.Add(s.soulID);
            await s.ApplyEffectAsync(ownerContext);

            OnDeckChanged?.Invoke();
        }

        public async UniTask TriggerOnParryAsync(GameObject attackerContext, GameObject enemyContext = null)
        {
            var s = GetNextUnusedSoul(SoulTriggerType.Parry);
            if (s == null) return;

            usedSoulIDs.Add(s.soulID);
            await s.ApplyEffectAsync(attackerContext, enemyContext);

            OnDeckChanged?.Invoke();
        }

        #endregion
    }
}
