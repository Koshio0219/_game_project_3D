using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Action
{
    public class AutoRotate : MonoBehaviour
    {
        public enum Axis {X,Y,Z}
        public Axis aroundAxis = Axis.Z;
        public float angleSpeed = 180f;

        private Vector3 aroundVector3;

        void Start()
        {
            switch (aroundAxis)
            {
                case Axis.X:
                    aroundVector3 = Vector3.right;break;
                case Axis.Y:
                    aroundVector3 = Vector3.up;break;
                case Axis.Z:
                    aroundVector3 = Vector3.forward;break;
            }
        }

        private void FixedUpdate()
        {
            transform.Rotate(aroundVector3, Time.fixedDeltaTime * angleSpeed);
        }
    }
}

