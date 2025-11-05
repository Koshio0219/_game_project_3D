using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;

namespace Game.Hud
{
    public class UIRevealAndBreath : MonoBehaviour
    {
        [Header("UI目标")]
        public Image targetImage;

        [Header("出现动画参数")]
        public float revealDuration = 1.5f;   // 从上往下显现时间
        public float finalAlpha = 0.9f;       // 显现后目标透明度

        [Header("呼吸动画参数")]
        public float breathMinAlpha = 0.6f;
        public float breathMaxAlpha = 0.9f;
        public float breathDuration = 1.5f;

        private Tween breathingTween;

        /// <summary>
        /// 从上往下缓缓出现（利用 fillAmount 实现）
        /// </summary>
        public void PlayReveal(Image targetImage, UnityAction onComplete = null)
        {
            if (targetImage == null)
            {
                Debug.LogWarning("未指定 Image 目标，替换为本地目标");
                if (this.targetImage == null)
                    return;
                targetImage = this.targetImage;
            }

            // 确保是可填充类型
            targetImage.type = Image.Type.Filled;
            targetImage.fillMethod = Image.FillMethod.Vertical;
            targetImage.fillOrigin = (int)Image.OriginVertical.Top;

            // 初始化
            targetImage.fillAmount = 0f;
            Color color = targetImage.color;
            color.a = 0f;
            targetImage.color = color;

            // 淡入 + 填充动画
            Sequence seq = DOTween.Sequence();
            seq.Append(targetImage.DOFade(finalAlpha, revealDuration * 0.8f).SetEase(Ease.InOutSine));
            seq.Join(DOTween.To(() => targetImage.fillAmount, x => targetImage.fillAmount = x, 1f, revealDuration).SetEase(Ease.OutCubic));
            seq.OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// 开始呼吸动画
        /// </summary>
        public void StartBreathing(Image targetImage, UnityAction onStart = null)
        {
            if (targetImage == null)
            {
                Debug.LogWarning("未指定 Image 目标，替换为本地目标");
                if (this.targetImage == null)
                    return;
                targetImage = this.targetImage;
            }

            onStart?.Invoke();

            breathingTween?.Kill();
            breathingTween = targetImage.DOFade(breathMinAlpha, breathDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        /// <summary>
        /// 停止呼吸动画
        /// </summary>
        public void StopBreathing()
        {
            breathingTween?.Kill();
            breathingTween = null;
        }
    }
}
