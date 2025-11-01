using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public enum EnemyTypeID
    {
        _101,_102,_103,_104,_105,_106,_107,_108,_109,_1010
    }

    public enum EnemyRaceType
    {
        Slime,
        Guard,
        Ghost,
    }

    public enum EnemyAttackType
    {
        Close,
        Remote,
        Boss
    }

    public enum EnemyState
    {
        Idle,
        Moving,
        Attack,
        Hit,
        Dead
    }

    public enum AttackState
    {
        Normal,
        Skill
    }

    [System.Serializable]
    public struct EnemyBaseProp:IInit
    {
        public float maxHp;
        public float Hp { get; private set; }
        public float attack;
        public int killPoint;

        public void Init()
        {
            Hp = maxHp;
        }

        //public EnemyBaseProp() => Hp = maxHp;
    }

    [System.Serializable]
    public struct EnemyUnitData: IInit
    {
        public int InsId { get; private set; }
        public EnemyRaceType raceType;
        public EnemyAttackType attackType;
        public EnemyBaseProp prop;
        public List<int> skillIds;

        //public EnemyUnitData() => InsId = GameHelper.GetId();

        public void Init()
        {
            InsId = GameHelper.GetId();
            prop.Init();
        }
    }
}