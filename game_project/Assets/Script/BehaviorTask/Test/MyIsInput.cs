using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

namespace Game.BehaviorTask
{
    public class MyIsInput : Conditional
    {
        public override TaskStatus OnUpdate()
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");

            if (inputX == 0 && inputZ == 0)
            {
                return TaskStatus.Failure;
            }
            else
            {
                Debug.Log("x: " + inputX + "z: " + inputZ);
                return TaskStatus.Success;
            }
        }
    }
}

