using Game.Base;
using Game.Framework;
using System.Collections;
using UnityEngine;

namespace Game.Unit
{
    [RequireComponent(typeof(Bullet))]
    public class BulletHit : MonoBehaviour, IInit<(int, float)>
    {
        private float damage;
        private int sourceId = -1;

        private Bullet bullet;

        private void Awake()
        {
            bullet = GetComponent<Bullet>();
        }

        public void Init((int, float) data)
        {
            sourceId = data.Item1;
            damage = data.Item2;
        }

        private void OnTriggerEnter(Collider other)
        {
            var up = other.transform.GetRootParent();

            //recycle
            if (up.TryGetComponent<Bullet>(out _)) return;
            var enemy = GameManager.stageManager.GetEnemy(sourceId);
            if (enemy != null && up.gameObject.GetInstanceID() == enemy.gameObject.GetInstanceID())
                return;
            var player = GameManager.stageManager.GetPlayer(sourceId);
            if (player != null && up.gameObject.GetInstanceID() == player.gameObject.GetInstanceID())
                return;
            bullet.Recycle();

            //damage
            if (!up.TryGetComponent<IDamageable>(out var damageable)) return;
            damageable.Hit(sourceId, damage);
            EventQueueSystem.QueueEvent(new SendDamageEvent(sourceId, up.gameObject, damage));
        }
    }
}