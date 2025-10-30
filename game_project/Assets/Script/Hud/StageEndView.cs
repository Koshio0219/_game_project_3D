using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.Hud
{
    public class StageEndView : HudView
    {
        public GameObject battleClearPage;
        public GameObject gameOverPage;
        public TextMeshProUGUI text_battleClear;

        public void Win()
        {
            battleClearPage.Show();
            gameOverPage.Hide();
            ShowBattleClearText();
        }

        public void Lose()
        {
            battleClearPage.Hide();
            gameOverPage.Show();
        }

        private void ShowBattleClearText()
        {
            //string text1 = $"ステージ {GameManager.Instance.LevelIdx+1} 結果発表 ：\n";
            //var text2 = "";
            //var pointList = GameManager.pointManager.mapItemToPoint;
            //foreach (var one in pointList)
            //{
            //    if (one.Value == 0)
            //        continue;

            //    switch (one.Key)
            //    {
            //        case GetPointItem.KillSlime:
            //            text2 += $"スライムを倒した：{one.Value} p\n";
            //            break;
            //        case GetPointItem.KillGuard:
            //            text2 += $"ガードを倒した：{one.Value} p\n";
            //            break;
            //        case GetPointItem.KillGhost:
            //            text2 += $"バルーン幽霊を倒した：{one.Value} p\n";
            //            break;
            //        case GetPointItem.GetGem:
            //            text2 += $"宝箱を手に入れた：{one.Value} p\n";
            //            break;
            //        case GetPointItem.ReachGoal:
            //            text2 += $"残り時間ボーナス：{one.Value} p\n";
            //            break;
            //    }
            //}

            //var text3 = $"最終ポイント：{GameManager.pointManager.CurrentPoint} p\n";
            //var text4 = GameManager.stageManager.IsLastStage() ? "ゲームクリアおめでとうございます！\n丸ボダンを押してメインメニューに戻ってください" : "バトルクリアおめでとうございます！\n丸ボダンを押して次のステージに進んでください";

            //text_battleClear.text = text1 + text2 + text3 + text4;
        }
    }
}

