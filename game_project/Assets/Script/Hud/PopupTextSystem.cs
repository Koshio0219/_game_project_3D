using Game.Base;
using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Hud
{
    public class PopupTextSystem : MonoBehaviour
    {
        public float textHeight = 3f;
        private void Awake()
        {
            EventQueueSystem.AddListener<PopupTextEvent>(PopupTextHandler);
        }

        private void OnDestroy()
        {
            EventQueueSystem.RemoveListener<PopupTextEvent>(PopupTextHandler);
        }

        private void PopupTextHandler(PopupTextEvent e)
        {
            var prefab = GameManager.Instance.gameData.hudConfig.popupTextPrefab;
            if (prefab == null) return;
            var ins = GameObjectPool.Instance.GetObj(prefab);
            ins.transform.SetParent(null);
            ins.transform.position = e.target.position +
                Vector3.up * UnityEngine.Random.Range(textHeight - .15f, textHeight + .15f) +
                Vector3.right * UnityEngine.Random.Range(-.3f, .3f) +
                Vector3.back * UnityEngine.Random.Range(0.2f, 0.6f);
            var com = ins.GetComponent<PopupText>();
            com.Setup(e.num, e.color);
        }
    }
}

