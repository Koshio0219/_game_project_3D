using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.InputSystem;

namespace Game.BehaviorTask
{
    public class MyInputMove : BehaviorDesigner.Runtime.Tasks.Action
    {
        public SharedFloat speed = 5f;
        public override TaskStatus OnUpdate()
        {
            if (Gamepad.all.Count > 0)
            {
                var input = Gamepad.all[0].leftStick.ReadValue();
                if (input!=Vector2.zero)
                {
                    Vector3 movement = new Vector3(input.x, 0, input.y);
                    transform.Translate(movement * Time.deltaTime * speed.Value);
                    return TaskStatus.Running;
                }
                return TaskStatus.Success;
            }
            else
            {
                float inputX = Input.GetAxis("Horizontal");
                float inputZ = Input.GetAxis("Vertical");
                if (inputX != 0 || inputZ != 0)
                {
                    Vector3 movement = new Vector3(inputX, 0, inputZ);
                    transform.Translate(movement * Time.deltaTime * speed.Value);
                    return TaskStatus.Running;
                }
                return TaskStatus.Success;
            }

            //return TaskStatus.Failure;
        }
    }
}

