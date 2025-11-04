using Game.Base;
using Game.Framework;
using System;
using UnityEngine;

namespace Game.Player
{
    public class PlayerPropManager : Singleton<PlayerPropManager>,IInit<PlayerData>
    {
        public PlayerData Prop { get;private set; }

        public void Init(PlayerData data)
        {
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
    }
}
