using System.Collections;
using UnityEngine;

namespace Game.Player
{
    public enum WeaponHandType
    {
        Left,
        Right,
        Both
    }

    public enum WeaponAttackType
    {
        Melee,
        Remote
    }

    public class Weapon : MonoBehaviour
    {
        public int WeaponID { get; private set; }
        public virtual PlayerData AddProp { get;private set; }

        public virtual WeaponHandType HandType { get; private set; }
        public virtual Vector3 SpawnPosOffse { get; private set; }

        public virtual WeaponAttackType AttackType { get; private set; }

        public virtual void NormalAttack()
        {
            Debug.Log($"ID: {WeaponID} - Normal Attack");
        }

        public virtual void SpecialAttack()
        {
            Debug.Log($"ID: {WeaponID} - Special Attack");
        }
    }
}