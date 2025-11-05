using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Soul;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using Game.Framework;

namespace Game.Soul.UI
{
    public class SoulLibraryUI : MonoBehaviour
    {
        public RectTransform contentParent; // 容器（VerticalLayoutGroup）
        public GameObject soulSlotPrefab; // 带 SoulSlot 组件的预制
        public GameObject tooltipObject;
        public TextMeshProUGUI tooltipText;

        private readonly List<SoulSlot> slots = new();
        private readonly Dictionary<SoulTriggerType, string> mapSoulToString = new()
        {
            {SoulTriggerType.Inherent, "固有剑魂"},
            {SoulTriggerType.Dodge,"闪避剑魂" },
            {SoulTriggerType.Parry,"招架剑魂" }
        };     

        private SwordSoul draggingSoulData;
        private SoulSlot draggingSlot;

        public bool IsDragging => draggingSlot != null;

        void Start()
        {
            BuildSlotsFromManager();
            SwordSoulManager.Instance.OnDeckChanged += BuildSlotsFromManager;
            HideTooltip();

            //fix bug: Tooltip 不阻挡鼠标事件
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
            for (int i = 0; i < deck.Count; i++)
            {
                var go = GameObjectPool.Instance.GetObj(soulSlotPrefab, contentParent);
                var slot = go.GetComponent<SoulSlot>();
                slot.Init(deck[i], i, this);
                slots.Add(slot);
            }
        }

        void ClearSlots()
        {
            foreach (var s in slots)
            {
                if (s != null) GameObjectPool.Instance.RecycleObj(s.gameObject);
            }
            slots.Clear();
        }

        private SoulSlot potentialTargetSlot;

        public void HandleDrag(SoulSlot slot, PointerEventData eventData)
        {
            draggingSlot = slot;
            draggingSoulData = SwordSoulManager.Instance.currentDeck[slot.index];
            potentialTargetSlot = null;

            // 找到当前鼠标所在的另一个 slot
            foreach (var srt in slots)
            {
                if (srt == slot) continue;
                var r = srt.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(r, eventData.position, eventData.enterEventCamera))
                {
                    potentialTargetSlot = srt;
                    break;
                }
            }
        }

        public void EndDrag(SoulSlot slot, PointerEventData eventData)
        {
            if (potentialTargetSlot != null && potentialTargetSlot != slot)
            {
                int a = slot.index;
                int b = potentialTargetSlot.index;
                SwordSoulManager.Instance.MoveSoul(a, b);
            }

            // 重建 UI
            BuildSlotsFromManager();
            draggingSlot = null;
            draggingSoulData = null;
            potentialTargetSlot = null;
        }

        public void ShowTooltip(SoulSlot slot, SwordSoul data)
        {
            tooltipObject.SetActive(true);
            tooltipText.text = $"[{mapSoulToString[data.triggerType]}] {data.soulID}\n{data.description}";
            // 简单定位到 slot 右侧
            var r = slot.GetComponent<RectTransform>();
            tooltipObject.GetComponent<RectTransform>().position = r.position + Vector3.right * 120;
        }

        public void HideTooltip()
        {
            tooltipObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (SwordSoulManager.Instance != null) SwordSoulManager.Instance.OnDeckChanged -= BuildSlotsFromManager;
        }
    }
}
