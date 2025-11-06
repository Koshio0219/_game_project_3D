using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Game.Framework;
using Game.Soul;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Soul.UI
{
    public class SoulLibraryUI : MonoBehaviour
    {
        [Header("剑魂容器 (HorizontalLayoutGroup)")]
        public RectTransform inherentParent;
        public RectTransform dodgeParent;
        public RectTransform parryParent;

        [Header("预制与提示UI")]
        public GameObject soulSlotPrefab;
        public GameObject tooltipObject;
        public TextMeshProUGUI tooltipText;

        private readonly List<SoulSlot> slots = new();

        void Start()
        {
            BuildSlotsFromManager();
            SwordSoulManager.Instance.OnDeckChanged += BuildSlotsFromManager;
            HideTooltip();

            // Tooltip 不阻挡鼠标事件
            if (tooltipObject.TryGetComponent<CanvasGroup>(out var cg))
                cg.blocksRaycasts = false;
            else
            {
                var cgNew = tooltipObject.AddComponent<CanvasGroup>();
                cgNew.blocksRaycasts = false;
            }
        }

        public void BuildSlotsFromManager()
        {
            ClearSlots();

            var deck = SwordSoulManager.Instance.currentDeck;

            foreach (var soul in deck)
            {
                RectTransform parent = soul.triggerType switch
                {
                    SoulTriggerType.Inherent => inherentParent,
                    SoulTriggerType.Dodge => dodgeParent,
                    SoulTriggerType.Parry => parryParent,
                    _ => inherentParent
                };

                var go = GameObjectPool.Instance.GetObj(soulSlotPrefab, parent);
                var slot = go.GetComponent<SoulSlot>();
                slot.Init(soul, this);
                slots.Add(slot);
            }
        }

        void ClearSlots()
        {
            foreach (var s in slots)
            {
                if (s != null)
                    GameObjectPool.Instance.RecycleObj(s.gameObject);
            }
            slots.Clear();
        }

        public void MoveSoulInCategory(SwordSoul soul, int direction)
        {
            var deck = SwordSoulManager.Instance.currentDeck;
            var category = soul.triggerType;

            // 找出同类型的剑魂
            var sameTypeSouls = deck.Where(s => s.triggerType == category).ToList();
            int currentIndex = sameTypeSouls.IndexOf(soul);
            int newIndex = currentIndex + direction;
            if (newIndex < 0 || newIndex >= sameTypeSouls.Count)
                return;

            // 在总体列表中交换位置
            int globalA = deck.IndexOf(sameTypeSouls[currentIndex]);
            int globalB = deck.IndexOf(sameTypeSouls[newIndex]);
            SwordSoulManager.Instance.MoveSoul(globalA, globalB);

            // ✅ 只调整 UI 顺序，而不重新 Build
            SwapUISlots(globalA, globalB,category);
        }

        // 新增方法：交换UI层的Slot顺序
        private void SwapUISlots(int indexA, int indexB,SoulTriggerType triggerType)
        {

            RectTransform parent =null; // 你的Slot容器
            switch (triggerType)
            {
                case SoulTriggerType.Inherent:
                    parent = inherentParent;
                    break;
                case SoulTriggerType.Dodge:
                    parent = dodgeParent;
                    break;
                case SoulTriggerType.Parry:
                    parent = parryParent;
                    break;
                default:
                    break;
            }
        
            if (indexA < 0 || indexA >= parent.childCount || indexB < 0 || indexB >= parent.childCount)
                return;

            var slotA = parent.GetChild(indexA);
            var slotB = parent.GetChild(indexB);

            int siblingA = slotA.GetSiblingIndex();
            int siblingB = slotB.GetSiblingIndex();

            slotA.SetSiblingIndex(siblingB);
            slotB.SetSiblingIndex(siblingA);
        }

        public void ShowTooltip(SoulSlot slot, SwordSoul data)
        {
            tooltipObject.SetActive(true);
            tooltipText.text = $"[{data.soulID}]\n{data.description}";

            var r = slot.GetComponent<RectTransform>();
            tooltipObject.GetComponent<RectTransform>().position = r.position + new Vector3(80f, 0, 0);
        }

        public void HideTooltip()
        {
            tooltipObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (SwordSoulManager.Instance != null)
                SwordSoulManager.Instance.OnDeckChanged -= BuildSlotsFromManager;
        }
    }
}
