using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

namespace Game.Framework
{
    public class GameEvent { }
    public class EventQueueSystem : MonoSingleton<EventQueueSystem>
    {
        public delegate void EventDelegate<T>(T e) where T : GameEvent;

        private delegate void InternalEventDelegate(GameEvent e);

        private readonly Dictionary<Type, InternalEventDelegate> delegates = new();
        private readonly Dictionary<Delegate, InternalEventDelegate> delegateLookup = new();
        private readonly Dictionary<InternalEventDelegate, Delegate> delegateLookOnce = new();

        private readonly Queue eventQueue = new();

        //thread locker
        private static readonly Mutex mutex = new();

        public bool bLimitQueueProcessing = false;
        public float limitQueueTime = 0.1f;

        //注册侦听事件（持续）
        //add a listener(長続き)
        public static void AddListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            Instance.AddDelegate(del);
        }

        //注册侦听事件（一次）
        //add a listener(only one)
        public static void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent
        {
            var result = Instance.AddDelegate(del);
            if (result != null)
                Instance.delegateLookOnce[result] = del;
        }

        //判定侦听事件是否存在
        //Listenerの存在を判断する
        public static bool HasListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            return Instance.delegateLookup.ContainsKey(del);
        }

        //移除侦听事件
        //ListenerをRemoveする
        public static void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (Instance == null)
                return;
            if (Instance.delegateLookup.TryGetValue(del, out InternalEventDelegate eventDelegate))
            {
                if (Instance.delegates.TryGetValue(typeof(T), out InternalEventDelegate temp))
                {
                    temp -= eventDelegate;
                    if (temp == null)
                        Instance.delegates.Remove(typeof(T));
                    else
                        Instance.delegates[typeof(T)] = temp;
                }
                Instance.delegateLookup.Remove(del);
            }
        }

        public static void RemoveAll()
        {
            if (Instance != null)
            {
                Instance.delegates.Clear();
                Instance.delegateLookup.Clear();
                Instance.delegateLookOnce.Clear();
            }
        }

        private InternalEventDelegate AddDelegate<T>(EventDelegate<T> del) where T : GameEvent
        {
            if (delegateLookup.ContainsKey(del))
                return null;
            void eventDelegate(GameEvent e) => del((T)e);
            delegateLookup[del] = eventDelegate;

            if (delegates.TryGetValue(typeof(T), out InternalEventDelegate temp))
                delegates[typeof(T)] = temp += eventDelegate;
            else
                delegates[typeof(T)] = eventDelegate;
            return eventDelegate;
        }

        //单个事件触发
        //eventを触発する
        private static void TriggerEvent(GameEvent e)
        {
            var type = e.GetType();
            if (Instance.delegates.TryGetValue(type, out InternalEventDelegate eventDelegate))
            {
                eventDelegate.Invoke(e);
                //移除单一侦听
                //もしonly oneのListenerであれば、そのListenerをRemoveする
                foreach (InternalEventDelegate item in Instance.delegates[type].GetInvocationList())
                {
                    if (Instance.delegateLookOnce.TryGetValue(item, out Delegate temp))
                    {
                        Instance.delegates[type] -= item;
                        if (Instance.delegates[type] == null)
                            Instance.delegates.Remove(type);
                        Instance.delegateLookup.Remove(temp);
                        Instance.delegateLookOnce.Remove(item);
                    }
                }
            }
        }

        //外部调用的推入事件队列接口
        //外部からのEventをQueueに入る
        public static void QueueEvent(GameEvent e)
        {
            if (!Instance.delegates.ContainsKey(e.GetType()))
                return;

            mutex.WaitOne();
            Instance.eventQueue.Enqueue(e);
            mutex.ReleaseMutex();
        }

        //事件队列触发处理
        //EventQueueの触発の管理する関数
        void Update()
        {
            float timer = 0.0f;
            while (eventQueue.Count > 0)
            {
                if (bLimitQueueProcessing)
                    if (timer > limitQueueTime)
                        return;
                var e = eventQueue.Dequeue() as GameEvent;
                TriggerEvent(e);
                if (bLimitQueueProcessing)
                    timer += Time.deltaTime;
            }
        }

        private void OnApplicationQuit()
        {
            RemoveAll();
            eventQueue.Clear();
        }
    }
}