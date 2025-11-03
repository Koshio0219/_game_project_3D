using Game.Base;
using System;
using UnityEngine;

namespace Game.Player
{
    [RequireComponent(typeof(Collider))]
    public class WeaponHitbox : MonoBehaviour
    {
        private PlayerAttackCtrl owner;
        private bool active = false;
        private bool isSkill = false;
        private float activationEndTime = 0.0f;
        private readonly System.Collections.Generic.HashSet<int> alreadyHit = new();
        private Collider col;

        public void Initialize(PlayerAttackCtrl owner)
        {
            this.owner = owner;
            col = GetComponent<Collider>();
            col.isTrigger = true;
            gameObject.SetActive(false);
        }

        public void Activate(float duration, bool isSkill)
        {
            if (owner == null) return;
            this.isSkill = isSkill;
            alreadyHit.Clear();
            active = true;
            activationEndTime = Time.time + Math.Max(0.01f, duration);
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (active && Time.time >= activationEndTime)
            {
                active = false;
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!active) return;
            if (other.gameObject == owner.gameObject) return;

            var root = other.transform.root.gameObject;
            int id = root.GetInstanceID();
            if (alreadyHit.Contains(id)) return;
            alreadyHit.Add(id);

            var dmgable = root.GetComponentInChildren<IDamageable>();
            if (dmgable != null)
            {
                float dmg = isSkill ? owner.PropManager.CalSkillAttackDamaage() : owner.PropManager.CalNormalAttackDamaage();
                dmgable.Hit(owner.InsId,dmg);
            }
        }
    }
}