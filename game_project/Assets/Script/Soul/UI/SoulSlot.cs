using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Game.Soul.UI
{
    public class SoulSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public Image background;

        [HideInInspector]
        public int index; // 在列表中的索引

        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private SoulLibraryUI libraryUI;
        private SwordSoul soulData;

        public void Init(SwordSoul data, int idx, SoulLibraryUI ui)
        {
            soulData = data;
            index = idx;
            libraryUI = ui;
            nameText.text = data.soulID;
            iconImage.sprite = data.icon;
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            transform.SetParent(canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
            //开始拖动时隐藏 Tooltip
            libraryUI.HideTooltip();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 限定：只能沿Y轴拖动
            Vector2 delta = eventData.delta / canvas.scaleFactor;
            rectTransform.anchoredPosition += new Vector2(0, delta.y);
            libraryUI.HandleDrag(this, eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            libraryUI.EndDrag(this, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // 拖动中不显示 Tooltip
            if (libraryUI.IsDragging) return;
            libraryUI.ShowTooltip(this, soulData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (libraryUI.IsDragging) return;
            libraryUI.HideTooltip();
        }


        // 刷新索引显示（外部调用）
        public void UpdateIndex(int idx)
        {
            index = idx;
        }
    }
}
