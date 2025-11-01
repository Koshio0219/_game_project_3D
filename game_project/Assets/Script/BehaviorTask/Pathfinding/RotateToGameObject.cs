using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine.AI;
using Game.Framework;

namespace Game.BehaviorTask
{
    public class RotateToGameObject : BehaviorDesigner.Runtime.Tasks.Action
    {
        [RequiredField]
        public SharedGameObject target;
        public SharedFloat speed = 2;
        public SharedFloat angleDifference = 5;
        public SharedVector3 upVector = Vector3.up;
        public bool waitActionFinish;

        public override TaskStatus OnUpdate()
        {
            if (target.Value == null) return TaskStatus.Failure;

            var dir = (target.Value.transform.position - transform.position).FixHeight().normalized;
            if (Vector3.Angle(dir, transform.forward) <= angleDifference.Value)
            {
                return TaskStatus.Success;
            }
         
            transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, dir, speed.Value * Time.deltaTime, 0), upVector.Value);
            if (!waitActionFinish)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

    }
}