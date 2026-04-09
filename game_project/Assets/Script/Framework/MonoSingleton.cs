using UnityEngine;

namespace Game.Framework
{
    /// <summary>
    /// 单例基类：支持场景预放置与运行时创建；可选跨场景保留；在Awake/Instance两侧都完成初始化。
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object locker = new();
        private static bool bAppQuitting = false;
        /// <summary>
        /// 是否跨场景保留。子类如需禁用持久化，override 返回 false 即可。
        /// </summary>
        protected virtual bool ShouldPersist => true;
        public static bool HasInstance
        {
            get
            {
                if (instance == null && !bAppQuitting)
                {
                    _ = Instance; // 尝试创建
                }
                return instance != null && !bAppQuitting;
            }
        }

        /// <summary>
        /// 全局访问点
        /// </summary>
        public static T Instance
        {
            get
            {
                if (bAppQuitting)
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorApplication.isPlaying)
                        Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} requested after quitting.");
#endif
                    return null;
                }

                lock (locker)
                {
                    if (instance == null)
                    {
#if UNITY_2022_2_OR_NEWER
                        var existing = FindAnyObjectByType<T>();
                        if (existing != null)
                        {
                            instance = existing;
                            PostBind(existing);
                            return instance;
                        }
#else
                        var instances = FindObjectsOfType<T>(true); // 包含Inactive
                        if (instances.Length > 0)
                        {
                            if (instances.Length > 1)
                                Debug.LogError($"[MonoSingleton] Multiple instances of {typeof(T)} found! Using the first one.");
                            instance = instances[0];
                            PostBind(instance);
                            return instance;
                        }
#endif
                        // 没找到，则创建
                        var go = new GameObject("(singleton) " + typeof(T).Name);
                        instance = go.AddComponent<T>();
                        PostBind(instance);
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// 在场景中预放置或运行时创建后，统一做：DontDestroyOnLoad + Init()
        /// </summary>
        private static void PostBind(T inst)
        {
            if (inst is MonoSingleton<T> mono)
            {
                if (mono.ShouldPersist)
                {
                    // 确保移出父节点，正确进入DontDestroyOnLoad场景
                    if (mono.transform.parent != null)
                        mono.transform.SetParent(null);
                    DontDestroyOnLoad(mono.gameObject);
                }
                mono.SafeInitOnce();
            }
        }

        private bool _inited = false;
        private void SafeInitOnce()
        {
            if (_inited) return;
            _inited = true;
            try { Init(); }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 保障：即使没有通过 Instance 访问，场景预放置对象在 Awake 也能完成单例绑定与初始化
        /// </summary>
        protected virtual void Awake()
        {
            if (bAppQuitting) return;

            if (instance == null)
            {
                instance = this as T;
                PostBind(instance);
            }
            else if (instance != this)
            {
                // 已有实例 → 这是重复体，销毁自己
                Debug.LogWarning($"[MonoSingleton] Duplicate instance of {typeof(T)} found on {name}, destroying this one.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            bAppQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            // 不是退出流程且销毁的是当前实例，则清空指针，避免 Editor 下关闭Domain Reload的脏引用
            if (!bAppQuitting && ReferenceEquals(instance, this))
                instance = null;
        }

        /// <summary>
        /// 子类初始化入口：保证只调用一次
        /// </summary>
        public virtual void Init() { }
    }
}
