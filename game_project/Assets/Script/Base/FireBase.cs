using Game.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Base
{
    public abstract class FireBase : MonoBehaviour, IFireOption
    {
        public abstract void FireBegin(int creatorId = 0, AttackState attackState = AttackState.Normal);
        public abstract void FireShut();

        public virtual FireBase SetDamage(float damage, float damageUpSpeed = 0f, int maxDamage = 0, float exCritRate = 0f)
        {
            return this;
        }

        public virtual FireBase SetTarget(GameObject tar)
        {
            return this;
        }

        public virtual FireBase SetFirePos(Transform pos)
        {
            return this;
        }
    }
}

