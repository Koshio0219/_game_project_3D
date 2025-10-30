using Game.Base;
using Game.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Hud
{
    public class StageShowView : HudView
    {
        public CustomBarView hpBar;
        public CustomBarView swordPointBar;

        [Header("Stats")]
        public TextMeshProUGUI atkText;
        public TextMeshProUGUI hitText;
        public TextMeshProUGUI critText;
        public TextMeshProUGUI critDmgText;

        public void InitHpbar(float hp)
        {
            hpBar.InitValueView($"{hp}/{hp}", "生命值");
        }

        public void InitSwordPointBar(int point)
        {
            swordPointBar.InitValueView($"{0}/{point}", "剑气");
        }

        public void UpdateHpbar(float lastHp,float nowHp,float maxHp)
        {
            hpBar.UpdateBarView($"{nowHp}/{maxHp}", lastHp, nowHp, maxHp);
        }

        public void UpdateSwordPointBar(int last,int now,int maxP)
        {
            swordPointBar.UpdateBarView($"{now}/{maxP}", last, now, maxP);
        }

        public void UpdateStats(PlayerData data)
        {
            atkText.text = $"攻击力: {data.AtkPoint:F1}";
            hitText.text = $"命中率: {data.HitRate * 100:F1}%";
            critText.text = $"暴击率: {data.CritRate * 100:F1}%";
            critDmgText.text = $"暴击伤害: {data.CritDmg * 100:F1}%";
        }
    }
}

