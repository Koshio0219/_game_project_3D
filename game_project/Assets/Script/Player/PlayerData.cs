using UnityEngine;

namespace Game.Player
{
    [System.Serializable]
    public class PlayerData
    {
        [SerializeField]
        private int _maxHP;
        public int MaxHP
        {
            get { return _maxHP; }
            set{  _maxHP = value;
            }
        }

        //current
        private int _HP;
        public int HP
        {
            get { return _HP; }
            set { _HP = value; }
        }

        [SerializeField]
        private float _AtkPoint;
        public float AtkPoint
        {
            get { return _AtkPoint; }
            set { _AtkPoint = value; }
        }

        //% 0-1
        [SerializeField]
        private float _HitRate;
        public float HitRate
        {
            get { return _HitRate; }
            set { _HitRate = value; }
        }
        //% 0-1
        [SerializeField]
        private float _CritRate;
        public float CritRate
        {
            get { return _CritRate; }
            set { _CritRate = value; }
        }

        //% >0
        [SerializeField]
        private float _CritDmg;
        public float CritDmg
        {
            get { return _CritDmg; }
            set { _CritDmg = value; }
        }

        //剑气上限
        [SerializeField]
        private int _maxSwordPoint;
        public int MaxSwordPoint
        {
            get { return _maxSwordPoint; }
            set { _maxSwordPoint = value; }
        }

        //剑气
        private int _SwordPoint;
        public int SwordPoint
        {
            get { return _SwordPoint; }
            set { _SwordPoint = value; }
        }

        public PlayerData(int maxSwordPoint=5)
        {
            _maxHP = 0;
            _HP = 0;
            _AtkPoint = 0;
            _HitRate = 0.0f;
            _CritRate = 0.0f;
            _CritDmg = 0.0f;
            _maxSwordPoint = maxSwordPoint;
            _SwordPoint = 0;
        }
    }
}
