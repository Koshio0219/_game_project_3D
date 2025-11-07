using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Base
{
    public class TimerTask : MonoBehaviour
    {
        [Header("计时器设置")]
        [Tooltip("时间间隔（秒）")]
        [SerializeField] private float interval = 1f;

        [Tooltip("是否循环执行")]
        [SerializeField] private bool isLoop = true;

        [Tooltip("是否在启动时自动开始计时")]
        [SerializeField] private bool autoStart = true;

        [SerializeField,Header("计时器事件")]
        private UnityEvent onTimerTick;  // 每次计时完成后触发的事件
        public event UnityAction OnTimerComplete;  // 计时器完成时触发的事件

        private bool _isRunning;
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            if (autoStart)
                StartTimer();
        }

        private void OnDisable()
        {
            StopTimer();
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public void StartTimer()
        {
            if (_isRunning) return;
            _cts = new CancellationTokenSource();
            RunTimerAsync(_cts.Token).Forget();
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void StopTimer()
        {
            _cts?.Cancel();
            _isRunning = false;
        }

        /// <summary>
        /// 异步计时逻辑
        /// </summary>
        private async UniTaskVoid RunTimerAsync(CancellationToken token)
        {
            _isRunning = true;

            try
            {
                do
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                    if (token.IsCancellationRequested) break;

                    onTimerTick?.Invoke();
                    OnTimerComplete?.Invoke();
                } while (isLoop && !token.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                // 被正常取消时不报错
            }
            finally
            {
                _isRunning = false;
            }
        }
    }
}
