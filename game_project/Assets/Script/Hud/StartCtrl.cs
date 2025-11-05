
using Game.Base;
using Game.Framework;
using KanKikuchi.AudioManager;
using UnityEngine;

namespace Game.Hud
{
    public class StartCtrl : HudCtrl<StartView>
    {
        public UIRevealAndBreath breathTarget;
        public GameObject optionsMenu;

        private void Awake()
        {
            View.startBtn.onClick.AddListener(OnStartBtnClick);
            View.optionsBtn.onClick.AddListener(OnOptionsBtnClick);
            View.BGM_Slider.onValueChanged.AddListener(OnBGMVolumeChanged);
            View.SE_Slider.onValueChanged.AddListener(OnSEVolumeChanged);
        }

        private void OnSEVolumeChanged(float arg0)
        {
            SEManager.Instance.ChangeBaseVolume(arg0);
            Debug.Log($"SE_Slider:{View.SE_Slider.value}");
        }

        private void OnBGMVolumeChanged(float arg0)
        {
            BGMManager.Instance.ChangeBaseVolume(arg0);
            Debug.Log($"BGM_Slider:{View.BGM_Slider.value}");
        }

        private void OnOptionsBtnClick()
        {
            if(optionsMenu.activeSelf)
            {
                GameHelper.FadeOut(optionsMenu, 0, () => optionsMenu.Hide());
                return;
            }
            optionsMenu.Show();
            GameHelper.FadeIn(optionsMenu);
        }

        private void OnStartBtnClick()
        {
            breathTarget.StopBreathing();
            SceneLoader.Instance.GoToStage();
            BGMSwitcher.FadeOutAndFadeIn(BGMPath.STUDIO_EIM);
        }

        private void OnDestroy()
        {
            View.startBtn.onClick.RemoveListener(OnStartBtnClick);
            View.optionsBtn.onClick.RemoveListener(OnOptionsBtnClick);
            View.BGM_Slider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
            View.SE_Slider.onValueChanged.RemoveListener(OnSEVolumeChanged);
        }

        private void Start()
        {
            View.BGM_Slider.value = BGMManager.Instance.GetBaseVolume();
            View.SE_Slider.value = SEManager.Instance.GetBaseVolume();

            Debug.Log($"BGM_Slider:{View.BGM_Slider.value}, SE_Slider:{View.SE_Slider.value}");

            optionsMenu.Hide();
            View.startBtn.gameObject.Hide();
            breathTarget.PlayReveal(null, () => 
            {
                breathTarget.StartBreathing(null);
                View.startBtn.gameObject.Show();
                GameHelper.FadeIn(View.startBtn.gameObject, .9f, null, 1f);
            });

            BGMSwitcher.FadeIn(Random.value > 0.5f ? BGMPath.BGMNEXT_STAGE : BGMPath.WAIT_STAGE);
        }
    }
}
