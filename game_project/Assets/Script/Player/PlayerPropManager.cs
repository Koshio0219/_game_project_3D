using Game.Base;
using Game.Framework;
using System;
using UnityEngine;

namespace Game.Player
{
    public class PlayerPropManager : Singleton<PlayerPropManager>,IInit<PlayerData>
    {
        public PlayerData Prop { get; private set; } = null;

        public void Init(PlayerData data)
        {
            if (Prop != null) return;
            Prop = data;
        }

        public float CalNormalAttackDamaage()
        {
            bool isHit = UnityEngine.Random.value < Prop.HitRate;
            if (!isHit) return 0;
            bool isCritical = UnityEngine.Random.value < Prop.CritRate;
            if (!isCritical) return Prop.AtkPoint;
            return Prop.AtkPoint * (1 + Prop.CritDmg);
        }

        public float CalSkillAttackDamaage()
        {
            return Prop.AtkPoint * Prop.SwordPoint * (1 + Prop.CritDmg);
        }

        public void AddProp(PlayerData addProp)
        {
            Prop += addProp;
        }

        public void RemoveProp(PlayerData removeProp)
        {
            Prop -= removeProp;
        }

        public void AddSwordPoint(int addPoint)
        {
            if (addPoint <= 0) return;
            Prop.SwordPoint += addPoint;
        }

        public void RemoveSwordPoint(int removePoint)
        {
            if (removePoint <= 0 || removePoint > Prop.SwordPoint) return;
            Prop.SwordPoint -= removePoint;
        }

        public bool CanUseSkill() => Prop.SwordPoint >= 3;

        public void ResetProp() => Prop = null;
    }
}
