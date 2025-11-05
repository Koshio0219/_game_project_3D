
using Game.Base;
using Game.Framework;
using System;

namespace Game.Hud
{
    public class StartCtrl : HudCtrl<StartView>
    {
        public UIRevealAndBreath breathTarget;

        private void Awake()
        {
            View.startBtn.onClick.AddListener(OnStartBtnClick);
        }

        private void OnStartBtnClick()
        {
            breathTarget.StopBreathing();
            SceneLoader.Instance.GoToStage();
        }

        private void OnDestroy()
        {
            View.startBtn.onClick.RemoveListener(OnStartBtnClick);
        }

        private void Start()
        {
            View.startBtn.gameObject.Hide();
            breathTarget.PlayReveal(null, () => 
            {
                breathTarget.StartBreathing(null);
                View.startBtn.gameObject.Show();
                GameHelper.FadeIn(View.startBtn.gameObject, .9f, null, 1f);
            });
        }
    }
}
