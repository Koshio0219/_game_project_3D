
using Game.Base;
using System;

namespace Game.Hud
{
    public class StartCtrl : HudCtrl<StartView>
    {
        private void Awake()
        {
            View.startBtn.onClick.AddListener(OnStartBtnClick);
        }

        private void OnStartBtnClick()
        {
            SceneLoader.Instance.GoToStage();
        }

        private void OnDestroy()
        {
            View.startBtn.onClick.RemoveListener(OnStartBtnClick);
        }
    }
}
