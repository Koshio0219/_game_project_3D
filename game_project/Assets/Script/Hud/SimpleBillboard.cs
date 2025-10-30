using Cysharp.Threading.Tasks;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Hud
{
    //make the obj face to camera
    public class SimpleBillboard : MonoBehaviour
    {
        public PlayerLoopTiming playerLoopTiming = PlayerLoopTiming.Update;
        private void Awake()
        {
            var main = Camera.main;
            UniTask.Void(async (_) =>
            {
                while (this && isActiveAndEnabled && !_.IsCancellationRequested)
                {
                    transform.forward = main.transform.forward;
                    await UniTask.DelayFrame(1, playerLoopTiming, this.GetCancellationTokenOnDestroy());
                }
            }, this.GetCancellationTokenOnDestroy());
        }

        //private void Start()
        //{
        //    UniTask.Void(async (_) =>
        //    {
        //        await UniTask.DelayFrame(3000, playerLoopTiming, this.GetCancellationTokenOnDestroy());
        //        EventQueueSystem.QueueEvent(new StageTimeUpEvent());

        //    }, this.GetCancellationTokenOnDestroy());
        //}
    }
}
