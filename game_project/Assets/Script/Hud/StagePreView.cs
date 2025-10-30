using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.Hud
{  
    public class StagePreView : HudView
    {
        public TextMeshProUGUI guideText;

        public override void OnShow()
        {
            SetGuide();
            base.OnShow();
        }

        public void SetGuide()
        {
            //string text1 = $"ステージ {GameManager.Instance.LevelIdx + 1}  開幕説明：\n";
            //var text2 = $"目標ポイント：{GameData.Instance.LevelConfig.levelDatas[GameManager.Instance.LevelIdx].goalPoint} p\n";
            //var text3 = $"制限時間：{GameData.Instance.LevelConfig.levelDatas[GameManager.Instance.LevelIdx].showTime} s\n";
            //var text4 = "制限時間内に\n目標ポイントを達成して\nゴールを探しましょう！\nTips: ポイント取得\n・敵を倒す　・宝箱入手";

            //guideText.text = text1 + text2 + text3 + text4;
        }
    }
}

