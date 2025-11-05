using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game.Hud
{
    public class HudView : MonoBehaviour
    {
        public HudState state;
        public HudType _type;

        public float Z_deep = 0;

        private Transform _tran = null;
        public Transform Tran
        {
            get
            {
                if(_tran == null)
                {
                    _tran = transform;
                }
                return _tran;
            }
        }

        public void OnEnable()
        {
            OnShow();
        }

        public void OnDisable()
        {
            OnClose();
        }

        public virtual void OnShow()
        {
            FadeIn();
        }

        public virtual void OnClose()
        {
            if (state == HudState.Hidden)
                gameObject.Hide();
            else if (state == HudState.Destroy)
                GameObjectPool.Instance.RecycleObj(gameObject);           
        }

        public virtual void OnRemove() { }

        public virtual void FadeIn() => GameHelper.FadeIn(gameObject);

        public virtual void FadeOut() => GameHelper.FadeOut(gameObject, 0, () => gameObject.Hide());

    }

    public enum HudState
    {
        None,
        Hidden,
        Show,
        Destroy,
    }

    public enum HudType
    {
        None,
        StartPanel,
        LoadingPanel,
        StagePrePanel,
        StageShowPanel,
        StageEndPanel
    }
}
