using Cysharp.Threading.Tasks;
using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Hud
{
    public class StageEndCtrl : HudCtrl<StageEndView>
    {
        private async void Start()
        {
            View.battleClearPage.Hide();
            View.gameOverPage.Hide();
            await UniTask.Delay(100);
            //switch (GameManager.stageManager.StageState)
            //{
            //    case StageStates.BattleClear:
            //        View.Win();
            //        break;
            //    case StageStates.GameOver:
            //        View.Lose();
            //        break;
            //}
        }
    }
}

