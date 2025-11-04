using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Framework;
using System;
using TMPro;
using UnityEngine;

namespace Game.Hud
{
    public class PopupText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float lifeTime = 1f;

        public void Setup(int num,Color color)
        {
            if (text == null) return;
            text.text =num == 0 ? "Miss!" : num.ToString();
            text.color = color;

            WaitRecycle().Forget();
            Move();
            Fade();
            ChangeSize();
        }

        private void Move()
        {
            transform.DOMoveY(transform.position.y + 0.6f, lifeTime).SetEase(Ease.InOutQuad);
        }

        private async UniTask WaitRecycle()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(lifeTime));
            if (this == null) return;
            GameObjectPool.Instance.RecycleObj(transform.root.gameObject);
        }

        private void Fade()
        {
            var par = text.transform.parent;
            var cg= par.gameObject.GetOrAddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.DOFade(1f, lifeTime);
        }

        private void ChangeSize()
        {
            transform.localScale = Vector3.one * .05f;
            transform.DOScale(.01f, 0.5f).SetEase(Ease.InOutExpo);
        }
    }
}

