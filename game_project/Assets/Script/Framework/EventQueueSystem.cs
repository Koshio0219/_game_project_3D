using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    public class GameEvent { }

    public class EventQueueSystem : MonoSingleton<EventQueueSystem>
    {
        public delegate void EventDelegate<T>(T e) where T : GameEvent;
        private delegate void InternalEventDelegate(GameEvent e);

        private readonly Dictionary<Type, InternalEventDelegate> _delegates = new();
        private readonly Dictionary<Delegate, InternalEventDelegate> _delegateLookup = new();
        private readonly Dictionary<InternalEventDelegate, Delegate> _delegateOnceLookup = new();

        private readonly Queue<GameEvent> _eventQueue = new();
        private static readonly object _lock = new();

        private static bool _isCleared = false;

        public bool LimitQueueProcessing = false;
        public float LimitQueueTime = 0.1f;

        // ---------------------------
        // Public APIs
        // ---------------------------

        public static void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (!HasInstance) return;
            Instance.AddDelegate(del, once: false);
        }

        public static void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (!HasInstance) return;
            Instance.AddDelegate(del, once: true);
        }

        public static bool HasListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            return HasInstance && Instance._delegateLookup.ContainsKey(del);
        }

        public static void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (!HasInstance || _isCleared) return;
            Instance.RemoveDelegate(del);
        }

        public static void RemoveAll()
        {
            if (!HasInstance) return;
            Instance._delegates.Clear();
            Instance._delegateLookup.Clear();
            Instance._delegateOnceLookup.Clear();
        }

        public static void QueueEvent(GameEvent e)
        {
            if (!HasInstance) return;
            lock (_lock)
            {
                Instance._eventQueue.Enqueue(e);
            }
        }

        // ---------------------------
        // Internal Delegate Handling
        // ---------------------------

        private InternalEventDelegate AddDelegate<T>(EventDelegate<T> del, bool once) where T : GameEvent
        {
            if (_delegateLookup.ContainsKey(del)) return null;

            void Wrapper(GameEvent e)
            {
                // 检查目标对象是否还存在
                if (del.Target is UnityEngine.Object uo && uo == null)
                {
                    RemoveDelegate(del);
                    return;
                }
                del((T)e);
            }

            var internalDel = (InternalEventDelegate)Wrapper;
            _delegateLookup[del] = internalDel;

            if (_delegates.TryGetValue(typeof(T), out var temp))
                _delegates[typeof(T)] = temp + internalDel;
            else
                _delegates[typeof(T)] = internalDel;

            if (once)
                _delegateOnceLookup[internalDel] = del;

            return internalDel;
        }

        private void RemoveDelegate<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (!_delegateLookup.TryGetValue(del, out var internalDel)) return;

            if (_delegates.TryGetValue(typeof(T), out var temp))
            {
                temp -= internalDel;
                if (temp == null)
                    _delegates.Remove(typeof(T));
                else
                    _delegates[typeof(T)] = temp;
            }

            _delegateLookup.Remove(del);
            _delegateOnceLookup.Remove(internalDel);
        }

        private void RemoveDelegate(Delegate del)
        {
            if (del == null) return;

            if (_delegateLookup.TryGetValue(del, out var internalDel))
            {
                foreach (var kv in _delegates)
                {
                    var temp = kv.Value - internalDel;
                    if (temp == null)
                        _delegates.Remove(kv.Key);
                    else
                        _delegates[kv.Key] = temp;
                }

                _delegateLookup.Remove(del);
                _delegateOnceLookup.Remove(internalDel);
            }
        }


        private void TriggerEvent(GameEvent e)
        {
            var type = e.GetType();
            if (!_delegates.TryGetValue(type, out var eventDel)) return;

            var invokeList = eventDel.GetInvocationList();
            foreach (InternalEventDelegate call in invokeList)
            {
                try
                {
                    // Unity 对象销毁保护
                    if (_delegateOnceLookup.ContainsKey(call))
                    {
                        call.Invoke(e);
                        RemoveDelegate(_delegateOnceLookup[call]);
                    }
                    else
                    {
                        call.Invoke(e);
                    }
                }
                catch (MissingReferenceException)
                {
                    // 监听目标被销毁，自动清理
                    RemoveByInternal(call);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[EventQueueSystem] Event {type.Name} threw exception: {ex.Message}");
                }
            }
        }

        private void RemoveByInternal(InternalEventDelegate call)
        {
            foreach (var kv in _delegates)
            {
                var temp = kv.Value - call;
                if (temp == null)
                    _delegates.Remove(kv.Key);
                else
                    _delegates[kv.Key] = temp;
            }

            if (_delegateOnceLookup.ContainsKey(call))
                _delegateLookup.Remove(_delegateOnceLookup[call]);
            _delegateOnceLookup.Remove(call);
        }

        // ---------------------------
        // Queue Execution
        // ---------------------------

        private void Update()
        {
            if (_eventQueue.Count == 0) return;

            float timer = 0f;
            while (_eventQueue.Count > 0)
            {
                if (LimitQueueProcessing && timer > LimitQueueTime)
                    break;

                GameEvent e;
                lock (_lock)
                {
                    e = _eventQueue.Dequeue();
                }

                TriggerEvent(e);
                if (LimitQueueProcessing)
                    timer += Time.deltaTime;
            }
        }

        // ---------------------------
        // Cleanup
        // ---------------------------

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            _eventQueue.Clear();
            RemoveAll();
            _isCleared = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _eventQueue.Clear();
            RemoveAll();
        }
    }
}
