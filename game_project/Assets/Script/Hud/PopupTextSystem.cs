using Game.Base;
using Game.Framework;
using UnityEngine;

namespace Game.Hud
{
    public class PopupTextSystem : MonoBehaviour
    {
        public float textHeight = .5f;
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
                Vector3.up * Random.Range(textHeight - .15f, textHeight + .15f) +
                Vector3.right * Random.Range(-.3f, .3f) +
                Vector3.back * Random.Range(0.2f, 0.6f);
            var com = ins.GetComponentInChildren<PopupText>();
            com.Setup(e.num);
        }
    }
}

