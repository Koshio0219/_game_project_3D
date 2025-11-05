using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using KanKikuchi.AudioManager;

namespace Game.Hud
{
    public class UIBeatVisualizer : MonoBehaviour
    {
        [Header("音频源")]
        public AudioSource audioSource;

        [Header("UI目标")]
        public RectTransform targetUI;

        [Header("缩放参数")]
        public float baseScale = 1f;
        public float maxScale = 1.3f;
        public float smoothSpeed = 5f; // 缩放平滑速度

        [Header("旋转参数")]
        public bool enableRotation = true;
        public float baseRotateSpeed = 30f;     // 基础旋转速度（度/秒）
        public float intensityRotateBoost = 100f; // 音量强度对应的额外旋转加成

        [Header("频谱采样大小")]
        public int sampleSize = 64;
        private float[] samples;

        void Start()
        {
            samples = new float[sampleSize];
            if(audioSource == null)
                audioSource = BGMManager.Instance.GetComponent<AudioSource>();
        }

        void Update()
        {
            if (audioSource == null || targetUI == null) return;

            // 获取频谱数据
            audioSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

            // 计算平均强度
            float sum = 0;
            for (int i = 0; i < sampleSize; i++)
                sum += samples[i];

            float intensity = Mathf.Clamp01(sum / sampleSize * 100f);

            // ---- 缩放逻辑 ----
            float targetScale = Mathf.Lerp(baseScale, maxScale, intensity);
            float scale = Mathf.Lerp(targetUI.localScale.x, targetScale, Time.deltaTime * smoothSpeed);
            targetUI.localScale = new Vector3(scale, scale, 1f);

            // ---- 旋转逻辑 ----
            if (enableRotation)
            {
                float rotateSpeed = baseRotateSpeed + intensity * intensityRotateBoost;
                targetUI.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            }
        }
    }
}
