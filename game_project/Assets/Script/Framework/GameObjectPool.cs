using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    public class GameObjectPool : Singleton<GameObjectPool>
    {
        private const int maxCount = 128;
        private readonly Dictionary<string, List<GameObject>> pool = new();
        private GameObject _pool;

        public GameObject GetObj(GameObject prefab)
        {
            string name = prefab.name;
            GameObject result = CheckPool(name);
            if (result != null)
                return result;

            result = Object.Instantiate(prefab);
            result.name = name;
            return result;
        }

        public GameObject GetObj(string name)
        {
            GameObject result = CheckPool(name);
            return result != null ? result : new GameObject(name);
        }

        public GameObject GetObj(GameObject prefab, Transform parent, bool worldPositionStays = true)
        {
            var result = GetObj(prefab);
            result.transform.SetParent(parent, worldPositionStays);
            return result;
        }

        private GameObject CheckPool(string name)
        {
            if (pool.TryGetValue(name, out var list) && list.Count > 0)
            {
                var result = list[0];
                list.RemoveAt(0);
                if (result != null)
                {
                    result.SetActive(true);
                    return result;
                }
                if (list.Count == 0) pool.Remove(name);
            }
            return null;
        }

        public void RecycleObj(GameObject obj, bool worldPositionStays = true)
        {
            if (obj == null) return;

            if (_pool == null)
            {
                _pool = new GameObject("_objectPool_") { hideFlags = HideFlags.HideInHierarchy };
            }

            obj.transform.SetParent(_pool.transform, worldPositionStays);
            obj.SetActive(false);

            if (!pool.TryGetValue(obj.name, out var list))
            {
                list = new List<GameObject>();
                pool[obj.name] = list;
            }

            if (list.Count < maxCount)
                list.Add(obj);
            else
                Object.Destroy(obj);
        }

        public void RecycleAllChildren(GameObject parent)
        {
            while (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0).gameObject;
                RecycleObj(child);
            }
        }

        public void Clear()
        {
            if (_pool != null)
            {
                foreach (Transform child in _pool.transform)
                    Object.Destroy(child.gameObject);

                Object.Destroy(_pool);
            }

            pool.Clear();
            _pool = null;
        }
    }
}
