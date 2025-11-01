using System.Collections;
using UnityEngine;

namespace Game.Base
{
    public interface IDamageable 
    {
        void Hit(int sourceId, float damage);
    }
}