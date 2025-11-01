using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Game.Framework;
using DG.Tweening;

namespace Game.BehaviorTask
{
    public class FlyAround : BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedTransformList targets;
        public SharedFloat flySpeed = 1f;
        public SharedFloat aroundDistance = 3f;
        public SharedFloat minHeight = 0f;
        public SharedFloat maxHeight = 5;
        public SharedInt maxRandomCount = 6;

        private Transform target;
        private TaskStatus taskStatus;

        public override void OnStart()
        {
            taskStatus = TaskStatus.Failure;
            if (targets == null || targets.Value.Count == 0) return;
            target = targets.Value.SelectOne();
            if (target == null) return;

            taskStatus = TaskStatus.Running;

            Vector3 tarPos = target.transform.position + .5f * aroundDistance.Value * Vector3.up;
            for(int i =0; i < maxRandomCount.Value; i++)
            {
                var randomPos = Random.insideUnitCircle * aroundDistance.Value;
                var tempPos = target.transform.position + (Vector3)randomPos;
                if (tempPos.y <= minHeight.Value && tempPos.y >= maxHeight.Value)
                {
                    tarPos = tempPos;
                    break;
                }
            }

            var distance = (transform.position - tarPos).magnitude;
            var moveTime = distance / flySpeed.Value;

            transform.DOMove(tarPos, moveTime).
                SetEase(Ease.InOutQuad).
                OnComplete(() =>
                taskStatus = TaskStatus.Success) ;
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
    }
}

