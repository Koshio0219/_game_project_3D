using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Game.Soul.UI
{
    public class SoulSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI引用")]
        public Image iconImage;
        public Button leftButton;
        public Button rightButton;
        public Image background;

        private SoulLibraryUI libraryUI;
        // 对外暴露：该 slot 绑定的 SwordSoul（便于查找）
        public SwordSoul BoundSoul { get; private set; }

        public void Init(SwordSoul data, SoulLibraryUI ui)
        {
            // 使用局部变量确保回调捕获的是正确的实例（避免闭包与对象池副作用）
            var captured = data;
            BoundSoul = captured;
            libraryUI = ui;

            if (iconImage != null)
                iconImage.sprite = captured.icon;

            // 清理并注册按钮（对象池复用安全）
            if (leftButton != null)
            {
                leftButton.onClick.RemoveAllListeners();
                leftButton.onClick.AddListener(() => libraryUI.MoveSoulInCategory(captured, -1));
            }

            if (rightButton != null)
            {
                rightButton.onClick.RemoveAllListeners();
                rightButton.onClick.AddListener(() => libraryUI.MoveSoulInCategory(captured, +1));
            }
        }

        private void OnDisable()
        {
            leftButton?.onClick.RemoveAllListeners();
            rightButton?.onClick.RemoveAllListeners();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            libraryUI.ShowTooltip(this, BoundSoul);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            libraryUI.HideTooltip();
        }
    }
}
