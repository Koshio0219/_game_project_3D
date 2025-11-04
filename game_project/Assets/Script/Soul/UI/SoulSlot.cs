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

        private Transform originalParent;
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
            nameText.text = data.name;
            iconImage.sprite = data.icon;
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
            originalParent = transform.parent;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            transform.SetParent(canvas.transform, true);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
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
            libraryUI.ShowTooltip(this, soulData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            libraryUI.HideTooltip();
        }

        // 刷新索引显示（外部调用）
        public void UpdateIndex(int idx)
        {
            index = idx;
        }
    }
}
