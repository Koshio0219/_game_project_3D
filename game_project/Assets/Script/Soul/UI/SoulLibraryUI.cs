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

        private SwordSoul draggingSoulData;
        private SoulSlot draggingSlot;

        void Start()
        {
            BuildSlotsFromManager();
            SwordSoulManager.Instance.OnDeckChanged += BuildSlotsFromManager;
            HideTooltip();
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

        // Called from SoulSlot.OnDrag
        public void HandleDrag(SoulSlot slot, PointerEventData eventData)
        {
            draggingSlot = slot;
            draggingSoulData = SwordSoulManager.Instance.currentDeck[slot.index];

            // auto-swap in list based on vertical position -> simple approach
            for (int i = 0; i < slots.Count; i++)
            {
                var srt = slots[i];
                if (srt == slot) continue;
                var r = srt.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(r, eventData.position, eventData.enterEventCamera))
                {
                    // swap visually and logically
                    int a = slot.index;
                    int b = srt.index;
                    if (a != b)
                    {
                        SwordSoulManager.Instance.MoveSoul(a, b);
                        // rebuild to refresh indexes/parents (simple)
                        BuildSlotsFromManager();
                        break;
                    }
                }
            }
        }

        public void EndDrag(SoulSlot slot, PointerEventData eventData)
        {
            // 将拖拽物体重新放回父节点（简单实现：重建UI即可）
            BuildSlotsFromManager();
            draggingSlot = null;
            draggingSoulData = null;
        }

        public void ShowTooltip(SoulSlot slot, SwordSoul data)
        {
            tooltipObject.SetActive(true);
            tooltipText.text = $"[{data.triggerType}] {data.name}\n{data.description}";
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
