using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Game.Framework
{
    /// <summary>
    /// 全局特效管理器，用于播放并自动回收特效。
    /// 示例调用：
    /// EffectManager.Instance.PlayEffect("FX_ParrySpark", position);
    /// </summary>
    public class EffectManager : MonoSingleton<EffectManager>
    {
        [Serializable]
        public class EffectEntry
        {
            public string key;
            public GameObject prefab;
        }

        [Header("预注册特效表")]
        public List<EffectEntry> effectPrefabs = new();

        private readonly Dictionary<string, GameObject> effectLookup = new();
        private readonly Dictionary<GameObject, float> activeEffects = new();

        protected override bool ShouldPersist => false;

        protected override void Awake()
        {
            base.Awake();

            foreach (var entry in effectPrefabs)
            {
                if (entry != null && entry.prefab != null)
                    effectLookup[entry.key] = entry.prefab;
            }
        }

        /// <summary>
        /// 播放特效（默认3秒后自动回收）
        /// </summary>
        public GameObject PlayEffect(string key, Vector3 position, float lifeTime = 3f)
        {
            if (!effectLookup.TryGetValue(key, out var prefab))
            {
                Debug.LogWarning($"[EffectManager] 未找到特效：{key}");
                return null;
            }

            var obj = GameObjectPool.Instance.GetObj(prefab);
            obj.transform.SetPositionAndRotation(position, Quaternion.identity);
            obj.SetActive(true);

            AutoRecycleAsync(obj, lifeTime).Forget();
            return obj;
        }

        /// <summary>
        /// 播放特效（跟随某个对象）
        /// </summary>
        public GameObject PlayEffectFollow(string key, Transform followTarget, Vector3 offset = default, float lifeTime = 3f)
        {
            var obj = PlayEffect(key, followTarget.position + offset, lifeTime);
            if (obj != null)
                obj.transform.SetParent(followTarget);
            return obj;
        }

        /// <summary>
        /// 异步延迟回收（使用UniTask代替Coroutine）
        /// </summary>
        private async UniTaskVoid AutoRecycleAsync(GameObject obj, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay));
            if (obj != null)
                GameObjectPool.Instance.RecycleObj(obj);
        }

        /// <summary>
        /// 手动停止并回收
        /// </summary>
        public void StopEffect(GameObject obj)
        {
            if (obj == null) return;
            GameObjectPool.Instance.RecycleObj(obj);
        }
    }
}
