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
            set{  _maxHP = value;}
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

        /// <summary>
        /// 属性叠加（常用于装备、Buff 等）
        /// </summary>
        public static PlayerData operator +(PlayerData a, PlayerData b)
        {
            if (a == null) return b?.Clone();
            if (b == null) return a.Clone();

            PlayerData result = new()
            {
                MaxHP = a._maxHP + b._maxHP,
                HP = a._HP + b._HP,
                AtkPoint = a._AtkPoint + b._AtkPoint,
                HitRate = Mathf.Clamp01(a._HitRate + b._HitRate),
                CritRate = Mathf.Clamp01(a._CritRate + b._CritRate),
                CritDmg = Mathf.Max(0, a._CritDmg + b._CritDmg),
                MaxSwordPoint = a._maxSwordPoint + b._maxSwordPoint,
                SwordPoint = Mathf.Clamp(a._SwordPoint + b._SwordPoint, 0, a._maxSwordPoint + b._maxSwordPoint)
            };
            return result;
        }

        /// <summary>
        /// 属性相减（常用于移除装备、Buff 等）
        /// </summary>
        public static PlayerData operator -(PlayerData a, PlayerData b)
        {
            if (a == null) return null;
            if (b == null) return a.Clone();

            PlayerData result = new()
            {
                MaxHP = Mathf.Max(0, a._maxHP - b._maxHP),
                HP = Mathf.Max(0, a._HP - b._HP),
                AtkPoint = Mathf.Max(0, a._AtkPoint - b._AtkPoint),
                HitRate = Mathf.Clamp01(a._HitRate - b._HitRate),
                CritRate = Mathf.Clamp01(a._CritRate - b._CritRate),
                CritDmg = Mathf.Max(0, a._CritDmg - b._CritDmg),
                MaxSwordPoint = Mathf.Max(0, a._maxSwordPoint - b._maxSwordPoint),
                SwordPoint = Mathf.Clamp(a._SwordPoint - b._SwordPoint, 0, a._maxSwordPoint)
            };
            return result;
        }

        /// <summary>
        /// 属性乘法（例如全体加成 1.1 倍）
        /// </summary>
        public static PlayerData operator *(PlayerData a, float factor)
        {
            if (a == null) return null;

            PlayerData result = new()
            {
                MaxHP = Mathf.RoundToInt(a._maxHP * factor),
                HP = Mathf.RoundToInt(a._HP * factor),
                AtkPoint = a._AtkPoint * factor,
                HitRate = Mathf.Clamp01(a._HitRate * factor),
                CritRate = Mathf.Clamp01(a._CritRate * factor),
                CritDmg = a._CritDmg * factor,
                MaxSwordPoint = Mathf.RoundToInt(a._maxSwordPoint * factor),
                SwordPoint = Mathf.RoundToInt(a._SwordPoint * factor)
            };
            return result;
        }

        /// <summary>
        /// 深拷贝（用于防止引用修改）
        /// </summary>
        public PlayerData Clone()
        {
            return new PlayerData(_maxSwordPoint)
            {
                _maxHP = _maxHP,
                _HP = _HP,
                _AtkPoint = _AtkPoint,
                _HitRate = _HitRate,
                _CritRate = _CritRate,
                _CritDmg = _CritDmg,
                _SwordPoint = _SwordPoint
            };
        }

        public override string ToString()
        {
            return $"HP:{_HP}/{_maxHP}, ATK:{_AtkPoint}, CRIT:{_CritRate * 100f}%, CRIT DMG:{_CritDmg}, HitRate:{_HitRate * 100f}%";
        }
    }
}
