using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Player
{
    public class PlayerData
    {
        private int _MaxHP;
        public int MaxHP
        {
            get { return _MaxHP; }
            set{  _MaxHP = value;
            }
        }

        //current
        private int _HP;
        public int HP
        {
            get { return _HP; }
            set { _HP = value; }
        }

        private float _AtkPoint;
        public float AtkPoint
        {
            get { return _AtkPoint; }
            set { _AtkPoint = value; }
        }

        //% 0-1
        private float _HitRate;
        public float HitRate
        {
            get { return _HitRate; }
            set { _HitRate = value; }
        }
        //% 0-1
        private float _CritRate;
        public float CritRate
        {
            get { return _CritRate; }
            set { _CritRate = value; }
        }

        //% >0
        private float _CritDmg;
        public float CritDmg
        {
            get { return _CritDmg; }
            set { _CritDmg = value; }
        }

        //剑气上限
        private int _maxSwordPoint;
        public int MaxSwordPoint
        {
            get { return _maxSwordPoint; }
            set { _maxSwordPoint = value; }
        }

        //剑气
        private int _swordPoint;
        public int SwordPoint
        {
            get { return _swordPoint; }
            set { _swordPoint = value; }
        }

        public PlayerData(int maxHP=100,int maxSwordPoint=5)
        {
            _MaxHP = maxHP;
            _HP = maxHP;
            _AtkPoint = 0;
            _HitRate = 0.0f;
            _CritRate = 0.0f;
            _CritDmg = 0.0f;
            _maxSwordPoint = maxSwordPoint;
            _swordPoint = 0;
        }
    }
}
