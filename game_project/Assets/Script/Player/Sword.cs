using System.Collections;
using UnityEngine;

namespace Game.Player
{
    public class Sword : Weapon
    {
        public override WeaponAttackType AttackType => WeaponAttackType.Melee;
    }
}