using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Cysharp.Threading.Tasks;
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

        // 全部slot缓存（便于回收）
        private readonly List<SoulSlot> allSlots = new();

        // 按类型分的slot列表（权威的UI顺序）
        private readonly Dictionary<SoulTriggerType, List<SoulSlot>> categorySlots = new()
        {
            { SoulTriggerType.Inherent, new List<SoulSlot>() },
            { SoulTriggerType.Dodge, new List<SoulSlot>() },
            { SoulTriggerType.Parry, new List<SoulSlot>() }
        };

        void Start()
        {
            BuildSlotsFromManager();
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
            // 回收所有旧slot
            foreach (var s in allSlots)
            {
                if (s != null) GameObjectPool.Instance.RecycleObj(s.gameObject);
                //if (s != null) Destroy(s.gameObject);
            }
            allSlots.Clear();
            foreach (var kv in categorySlots) kv.Value.Clear();

            var deck = SwordSoulManager.Instance.currentDeck;

            // 逐项生成并放到对应父容器与列表（保证 UI 顺序与 categorySlots 一致）
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
                //var go = Instantiate(soulSlotPrefab, parent);
                var slot = go.GetComponent<SoulSlot>();

                slot.Init(soul, this); // slot 内会安全捕获 data
                allSlots.Add(slot);
                categorySlots[soul.triggerType].Add(slot);
            }
        }

        // 主入口：按类型内部移动（direction: -1 向左/上, +1 向右/下）
        public void MoveSoulInCategory(SwordSoul soul, int direction)
        {
            var deck = SwordSoulManager.Instance.currentDeck;
            var category = soul.triggerType;

            var slotsList = categorySlots[category];
            int localIndex = slotsList.FindIndex(s => s.BoundSoul == soul);
            if (localIndex < 0) return;
            int newLocalIndex = localIndex + direction;
            if (newLocalIndex < 0 || newLocalIndex >= slotsList.Count) return;

            // 在全局 deck 中查到两个对应的全局索引（通过一次遍历映射，避免 IndexOf 错误）
            var globalIndices = new List<int>();
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i].triggerType == category)
                    globalIndices.Add(i);
            }

            if (globalIndices.Count != slotsList.Count)
            {
                // 严重不同步：退回并重建UI
                Debug.LogWarning("[SoulLibraryUI] category slots mismatch with deck -> rebuild UI");
                BuildSlotsFromManager();
                return;
            }

            int globalA = globalIndices[localIndex];
            int globalB = globalIndices[newLocalIndex];

            // 交换数据层
            SwordSoulManager.Instance.MoveSoul(globalA, globalB);

            // 交换 UI 层：list 内交换并交换 siblingIndex（用 transform.GetSiblingIndex())
            var slotA = slotsList[localIndex];
            var slotB = slotsList[newLocalIndex];

            // 交换 list 中位置
            slotsList[localIndex] = slotB;
            slotsList[newLocalIndex] = slotA;

            UniTask.Create(async () =>
            {
                // 延迟到下一帧执行（非常关键）
                await UniTask.NextFrame();

                int siblingA = slotA.transform.GetSiblingIndex();
                int siblingB = slotB.transform.GetSiblingIndex();
                slotA.transform.SetSiblingIndex(siblingB);
                slotB.transform.SetSiblingIndex(siblingA);
            });
        }

        public void ShowTooltip(SoulSlot slot, SwordSoul data)
        {
            tooltipObject.SetActive(true);
            tooltipText.text = $"[{data.soulID}]\n{data.description}";

            var r = slot.GetComponent<RectTransform>();
            tooltipObject.GetComponent<RectTransform>().position = r.position + new Vector3(50f, -360f, 0);
        }

        public void HideTooltip()
        {
            tooltipObject.SetActive(false);
        }
    }
}
