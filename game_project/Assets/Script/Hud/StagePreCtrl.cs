using Game.Base;
using Game.Framework;
using KanKikuchi.AudioManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Hud
{
    public class StagePreCtrl : HudCtrl<StagePreView>
    {
        private void Awake()
        {
            View.nextBtn.onClick.AddListener(OnNextBtnClick);
            BGMSwitcher.FadeOutAndFadeIn(Random.value > 0.5f ? BGMPath.WAIT_STAGE : BGMPath.BGMNEXT_STAGE);
        }

        private void OnNextBtnClick()
        {
            SceneLoader.Instance.GoToStage();
            BGMSwitcher.FadeOutAndFadeIn(BGMPath.STUDIO_EIM);
        }

        private void OnDestroy()
        {
            View.nextBtn.onClick.RemoveListener(OnNextBtnClick);
        }
    }
}

