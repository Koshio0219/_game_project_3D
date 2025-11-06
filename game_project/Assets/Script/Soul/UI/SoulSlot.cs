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
        private SwordSoul soulData;

        public void Init(SwordSoul data, SoulLibraryUI ui)
        {
            soulData = data;
            libraryUI = ui;

            if (iconImage != null)
                iconImage.sprite = data.icon;

            // fix bug:先清理旧监听，防止对象池复用时重复注册
            if (leftButton != null)
            {
                leftButton.onClick.RemoveAllListeners();
                leftButton.onClick.AddListener(() => libraryUI.MoveSoulInCategory(soulData, -1));
            }

            if (rightButton != null)
            {
                rightButton.onClick.RemoveAllListeners();
                rightButton.onClick.AddListener(() => libraryUI.MoveSoulInCategory(soulData, +1));
            }
        }

        private void OnDisable()
        {
            leftButton?.onClick.RemoveAllListeners();
            rightButton?.onClick.RemoveAllListeners();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            libraryUI.ShowTooltip(this, soulData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            libraryUI.HideTooltip();
        }
    }
}
