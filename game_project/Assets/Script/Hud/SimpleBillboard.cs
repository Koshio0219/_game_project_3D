using Cysharp.Threading.Tasks;
using Game.Framework;
using UnityEngine;

namespace Game.Hud
{
    public class SimpleBillboard : MonoBehaviour
    {
        public PlayerLoopTiming playerLoopTiming = PlayerLoopTiming.Update;

        private void OnEnable()
        {
            StartBillboardLoop().Forget();
        }

        private async UniTaskVoid StartBillboardLoop()
        {
            var main = Camera.main;
            var token = this.GetCancellationTokenOnDisable(); //  OnDisable Token
            while (this && isActiveAndEnabled && !token.IsCancellationRequested)
            {
                if (main != null)
                    transform.forward = main.transform.forward;
                await UniTask.DelayFrame(1, playerLoopTiming, token);
            }
        }
    }
}
