using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Game.Camera
{
    public class LockOnSystem: MonoBehaviour
    {
        public float lockOnRange = 20f;
        public LayerMask enemyLayer;

        public Transform FindClosestTarget()
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, lockOnRange, enemyLayer);
            if (enemies.Length == 0) return null;

            Transform closest = enemies[0].transform;
            float minAngle = float.MaxValue;

            foreach (var e in enemies)
            {
                Vector3 dir = e.transform.position - transform.position;
                float angle = Vector3.Angle(UnityEngine.Camera.main.transform.forward, dir);
                if (angle < minAngle)
                {
                    minAngle = angle;
                    closest = e.transform;
                }
            }

            return closest;
        }
    }
}
