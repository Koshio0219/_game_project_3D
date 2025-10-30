using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game.Hud
{
    public class CustomBarView : MonoBehaviour
    {
        public enum BarMode { Down, Up }
        [SerializeField] private BarMode barMode;
        [SerializeField] private Image img_mod;
        [SerializeField] private Image img_value;
        [SerializeField] private TextMeshProUGUI text_name;
        [SerializeField] private TextMeshProUGUI text_value;

        public void InitValueView(string _text_value, string _text_name = null)
        {
            text_name.text = _text_name ?? text_name.text;
            text_value.text = _text_value;

            img_value.fillAmount = barMode == BarMode.Down ? 1f : 0f;
            if (img_mod == null) return;
            img_mod.fillAmount = img_value.fillAmount;
        }

        public void UpdateBarView(string _text_value, float lastValue, float nowValue,float maxValue, float changeTime = .3f)
        {
            if (lastValue > nowValue)
                ValueDown(_text_value, nowValue,maxValue, changeTime);
            else if (lastValue < nowValue)
                ValueUp(_text_value, nowValue,maxValue);
        }

        private void ValueUp(string _text_value, float nowValue, float maxValue)
        {
            text_value.text = _text_value;
            img_value.fillAmount = nowValue * 1.0f / maxValue;
            if (img_mod == null) return;
            img_mod.fillAmount = img_value.fillAmount;
        }

        private void ValueDown(string _text_value, float nowValue, float maxValue, float changeTime,UnityAction callback = null)
        {
            text_value.text = _text_value;
            img_value.fillAmount = nowValue * 1.0f / maxValue;
            if (img_mod == null) return;
            DOTween.To(() => img_mod.fillAmount, (x) => img_mod.fillAmount = x, img_value.fillAmount, changeTime).OnComplete(() => callback?.Invoke());
        }
    }
}
