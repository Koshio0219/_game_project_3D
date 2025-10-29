using Game.Base;
using UnityEngine;

namespace Game.Framework
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static readonly object locker = new();
        private static bool bAppQuitting = false;

        public static T Instance
        {
            get
            {
                if (bAppQuitting)
                {
                    Debug.LogWarning($"[MonoSingleton] Instance of {typeof(T)} requested after quitting.");
                    return null;
                }

                lock (locker)
                {
                    if (instance == null)
                    {
                        var instances = FindObjectsByType<T>(FindObjectsSortMode.None);
                        if (instances.Length > 1)
                        {
                            Debug.LogError($"[MonoSingleton] Multiple instances of {typeof(T)} found!");
                            instance = instances[0];
                        }
                        else if (instances.Length == 1)
                        {
                            instance = instances[0];
                        }
                        else
                        {
                            var singleton = new GameObject("(singleton)" + typeof(T));
                            instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                            // 调用 Init()
                            if (instance is MonoSingleton<T> monoSingleton)
                            {
                                monoSingleton.Init();
                            }
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            bAppQuitting = true;
        }

        public virtual void Init() { }
    }
}

