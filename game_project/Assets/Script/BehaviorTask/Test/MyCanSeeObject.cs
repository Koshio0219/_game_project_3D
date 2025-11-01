using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Game.Framework;

namespace Game.BehaviorTask
{
    public class MyCanSeeObject : Conditional
    {
        public SharedTransformList targets;

        public SharedFloat radius = 5f;
        public SharedFloat angle = 120f;

        public SharedTransformList results;
        public SharedGameObject closest;

        private float sqrRadius;
        private Dictionary<Transform,float> sqrDis;

        public override TaskStatus OnUpdate()
        {
            results.Value.Clear();
            sqrDis.Clear();
            foreach (var target in targets.Value)
            {
                if (target == null)
                    continue;

                Vector3 offse = target.position - transform.position;
                var _sqrDis = offse.sqrMagnitude;
                if (_sqrDis > sqrRadius)
                    continue;

                float angle = Vector3.Angle(transform.forward, offse.FixHeight().normalized);
                if (angle > this.angle.Value * .5f)
                    continue;

                //Ray ray = new(transform.position, offse);
                //if (Physics.Raycast(ray, out RaycastHit info, radius.Value))
                //{
                //    if (info.collider.gameObject == target.gameObject)
                //    {
                        results.Value.Add(target);
                        sqrDis.Add(target, _sqrDis);
                //    }
                //}
            }

            if (results.Value.Count > 0)
            {
                closest.Value = FindClosest(sqrDis);
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        private GameObject FindClosest(Dictionary<Transform, float> transforms)
        {
            GameObject result = null;
            float max = float.PositiveInfinity;
            foreach (var item in transforms)
            {
                if (item.Value < max)
                {
                    max = item.Value;
                    result = item.Key.gameObject;
                }
            }
            return result;
        }

        public override void OnStart()
        {
            results.Value = new List<Transform>();
            sqrDis = new Dictionary<Transform, float>();
            sqrRadius = radius.Value * radius.Value;
        }
    }
}
