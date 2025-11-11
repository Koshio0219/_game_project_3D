using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Game.Hud
{
    public class UIBlinkEffect : MonoBehaviour
    {
        [Header("UI 目标")]
        public Graphic targetGraphic; // 可以是 Image / Text / TMP_Text

        [Header("闪烁参数")]
        public float minAlpha = 0.3f;      // 最低透明度
        public float maxAlpha = 1f;        // 最高透明度
        public float blinkDuration = 0.5f; // 单次闪烁时间

        [Header("自动启动选项")]
        public bool autoStartOnEnable = false; // 启用时自动开始闪烁

        private Tween blinkTween;

        private void OnEnable()
        {
            if (autoStartOnEnable)
                StartBlink();
        }

        private void OnDisable()
        {
            // 组件禁用时自动停止
            StopBlink();
        }

        /// <summary>
        /// 开始闪烁
        /// </summary>
        public void StartBlink()
        {
            if (targetGraphic == null)
            {
                Debug.LogWarning("未指定目标 UI Graphic");
                return;
            }

            blinkTween?.Kill();

            blinkTween = targetGraphic.DOFade(minAlpha, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// 停止闪烁并恢复为不透明
        /// </summary>
        public void StopBlink()
        {
            blinkTween?.Kill();
            blinkTween = null;

            if (targetGraphic != null)
            {
                Color c = targetGraphic.color;
                c.a = maxAlpha;
                targetGraphic.color = c;
            }
        }
    }

}
