using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using Game.Framework;

namespace Game.Hud
{
    /// <summary>
    /// 通用 UI 消息系统：基于对象池与 UniTask 的自动清理版本
    /// </summary>
    public class UIMessageSystem : MonoSingleton<UIMessageSystem>
    {
        [Header("消息容器（需挂 VerticalLayoutGroup）")]
        public Transform messageContainer;

        [Header("消息预制体（建议带有 TMP_Text）")]
        public GameObject messagePrefab;

        [Header("最大显示消息数量")]
        public int maxMessages = 10;

        [Header("每条消息存在时间（秒）"), Tooltip("设为0则不会自动隐藏")]
        public float lifeTime = 0f;

        private readonly Queue<GameObject> _messages = new();

        protected override bool ShouldPersist => false;

        public override void Init()
        {
            base.Init();
            if (messageContainer == null)
                Debug.LogWarning("UIMessageSystem: 未绑定 messageContainer");
            if (messagePrefab == null)
                Debug.LogWarning("UIMessageSystem: 未绑定 messagePrefab");
        }

        /// <summary>
        /// 添加一条消息（直接显示文本）
        /// </summary>
        public void AddMessage(string text)
        {
            if (messagePrefab == null || messageContainer == null)
            {
                Debug.LogWarning("UIMessageSystem: messagePrefab 或 messageContainer 未设置");
                return;
            }

            // 从对象池取出或创建
            var msg = GameObjectPool.Instance.GetObj(messagePrefab, messageContainer, false);

            // 自动寻找 TMP_Text 或普通 Text
            var tmp = msg.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.text = text;
            else
            {
                var txt = msg.GetComponentInChildren<UnityEngine.UI.Text>();
                if (txt != null)
                    txt.text = text;
            }

            msg.transform.SetAsLastSibling();
            msg.SetActive(true);

            _messages.Enqueue(msg);

            // 超出数量则回收最旧的
            if (_messages.Count > maxMessages)
            {
                var old = _messages.Dequeue();
                GameObjectPool.Instance.RecycleObj(old);
            }

            // 若有自动消失时间，使用 UniTask 延迟
            if (lifeTime > 0f)
                AutoRecycleAsync(msg, lifeTime).Forget();
        }

        /// <summary>
        /// 使用 UniTask 延迟自动回收消息
        /// </summary>
        private async UniTaskVoid AutoRecycleAsync(GameObject msg, float delay)
        {
            await UniTask.Delay((int)(delay * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());

            if (msg == null)
                return;

            if (_messages.Contains(msg))
            {
                var list = new List<GameObject>(_messages);
                list.Remove(msg);
                _messages.Clear();
                foreach (var go in list)
                    _messages.Enqueue(go);
            }

            GameObjectPool.Instance.RecycleObj(msg);
        }

        /// <summary>
        /// 清空所有消息
        /// </summary>
        public void ClearMessages()
        {
            while (_messages.Count > 0)
            {
                var msg = _messages.Dequeue();
                if (msg != null)
                    GameObjectPool.Instance.RecycleObj(msg);
            }
        }
    }
}
