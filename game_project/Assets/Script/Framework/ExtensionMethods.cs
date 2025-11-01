
using BehaviorDesigner.Runtime;

using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Radom = UnityEngine.Random;

namespace Game.Framework
{
    /// <summary>
    /// ゲームの中でよく利用されたExtensionMethodsのクラス
    /// </summary>
    public static class ExtensionMethods
    {
        #region Collections 関連

        /// <summary>
        /// random to select one from a list
        /// </summary>
        public static T SelectOne<T>(this List<T> list)
        {
            return list[Radom.Range(0, list.Count)];
        }

        public static List<T> CutSameItem<T>(this List<T> ts)
        {
            var temp = new List<T>();
            foreach (var t in ts)
                if (!temp.Contains(t))
                    temp.Add(t);
            return temp;
        }

        public static Tk Index<Tk, Tv>(this Dictionary<Tk, Tv> dictionary, int idx)
        {
            if (dictionary.Count < idx)
            {
                throw new Exception($"the input index:{idx} is bigger with the dictionary count: {dictionary.Count}！！");
            }

            var key = dictionary.Keys.ToList()[idx];
            return key;
        }

        public static void AddOrSet<Tk, Tv>(this Dictionary<Tk, Tv> temp, Tk key, Tv value)
        {
            if (temp.ContainsKey(key))
            {
                temp[key] = value;
            }
            else
            {
                temp.Add(key, value);
            }
        }

        public static void AddOrAddValue<Tk, Tv>(this Dictionary<Tk, Tv> temp, Tk key, Tv value) where Tv : struct
        {
            if (temp.ContainsKey(key))
            {
                temp[key] = GameHelper.Add(value, temp[key]);
            }
            else
            {
                temp.Add(key, value);
            }
        }

        /// <summary>
        /// converse a dictionary（key と value　は一つ一つに対応が必要 ）
        /// </summary>
        public static Dictionary<Tv, Tk> Converse<Tk, Tv>(this Dictionary<Tk, Tv> dic)
        {
            var temp = new Dictionary<Tv, Tk>();
            foreach (var kv in dic)
            {
                if (temp.ContainsKey(kv.Value))
                {
                    Debug.LogError("key と value　は一つ一つに対応が必要");
                    temp.Clear();
                    break;
                }
                temp.Add(kv.Value, kv.Key);
            }
            return temp;
        }

        public static void Add<Tk, Item>(this Dictionary<Tk, Stack<Item>> temp, Tk tk, Item item)
        {
            if (temp.ContainsKey(tk))
            {
                temp[tk].Push(item);
            }
            else
            {
                var tempStack = new Stack<Item>();
                tempStack.Push(item);
                temp.Add(tk, tempStack);
            }
        }

        public static bool ContainsValue<Tk, Tv>(this KeyValuePair<Tk, Tv>[] tks, Tv tv)
        {
            foreach (var item in tks)
            {
                if (item.Value.Equals(tv))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsKey<Tk, Tv>(this KeyValuePair<Tk, Tv>[] tks, Tk tk)
        {
            foreach (var item in tks)
            {
                if (item.Key.Equals(tk))
                {
                    return true;
                }
            }

            return false;
        }


        #endregion

        #region Component/Transform/GameObject 関連

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            if (obj.TryGetComponent<T>(out var component)) return component;
            return obj.AddComponent<T>();
        }

        public static Transform GetRootParent(this Transform transform)
        {
            if (transform.parent == null)
                return transform;
            else
                return transform.parent.GetRootParent();
        }

        public static void ResetLocal(this Transform transform, bool bChangeScale = true)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if (bChangeScale)
                transform.localScale = Vector3.one;
        }

        public static int GetParentIdx<T>(this T t) where T : Component
        {
            for (int i = 0; i < t.transform.parent.childCount; i++)
            {
                var temp = t.transform.parent.GetChild(i).GetComponent<T>();
                if (temp && temp == t)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void Show(this GameObject obj) 
        {
            if (obj.activeSelf) return;
            obj.SetActive(true);
        }

        public static void Hide(this GameObject obj)
        {
            if (obj.activeSelf)
                obj.SetActive(false);
        }

        public static void ShowAllChildren(this GameObject target)
        {
            for (int i = 0; i < target.transform.childCount; i++)
            {
                target.transform.GetChild(i).gameObject.Show();
            }
        }

        public static void HideAllChildren(this GameObject target)
        {
            for (int i = 0; i < target.transform.childCount; i++)
            {
                target.transform.GetChild(i).gameObject.Hide();
            }
        }

        public static List<GameObject> ShowChildrenCount(this GameObject target, int count)
        {
            var temp = new List<GameObject>();
            if (count > target.transform.childCount)
                return temp;

            target.HideAllChildren();
            for (int i = 0; i < count; i++)
            {
                target.transform.GetChild(i).gameObject.Show();
                temp.Add(target.transform.GetChild(i).gameObject);
            }

            return temp;
        }

        public static List<GameObject> GetAllChildren(this GameObject obj, bool includeSelf = false, bool includeHide = false)
        {
            var temp = new List<GameObject>();
            foreach (var child in obj.GetComponentsInChildren<Transform>(includeHide))
            {
                if (!includeSelf && obj.transform == child)
                {
                    continue;
                }

                temp.Add(child.gameObject);
            }

            return temp;
        }

        public static List<GameObject> GetAllParents(this GameObject obj, bool includeSelf = false)
        {
            var temp = new List<GameObject>();
            foreach (var par in obj.transform.GetComponentsInParent<Transform>())
            {
                if (!includeSelf && obj.transform == par)
                {
                    continue;
                }

                temp.Add(par.gameObject);
            }

            return temp;
        }

        public static List<GameObject> GetAllParentsAndChildren(this GameObject obj, bool includeSelf = false)
        {
            var temp = new List<GameObject>();
            var temp1 = obj.GetAllParents();
            var temp2 = obj.GetAllChildren(includeSelf);
            temp.AddRange(temp1);
            temp.AddRange(temp2);
            return temp;
        }

        public static void SetPositionX(this Transform transform, float x)
        {
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, z);
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, transform.localPosition.z);
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, z);
        }

        public static void SetEulerAnglesX(this Transform transform, float x)
        {
            transform.eulerAngles = new Vector3(x, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        public static void SetEulerAnglesY(this Transform transform, float y)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, y, transform.eulerAngles.z);
        }

        public static void SetEulerAnglesZ(this Transform transform, float z)
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
        }

        public static void SetLocalScaleX(this Transform transform, float x)
        {
            transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
        }

        public static void SetLocalScaleY(this Transform transform, float y)
        {
            transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
        }

        public static void SetLocalScaleZ(this Transform transform, float z)
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
        }

        public static Vector3 FixHeight(this Vector3 vector,float height = 0f)
        {
            return new Vector3(vector.x, height, vector.z);
        }

        #endregion

        #region Camera関連

        /// <summary>
        /// 判断一个世界坐标点是否在相机视野内。
        /// </summary>
        public static bool IsVisibleInCamera(
            this Camera camera,
            Vector3 worldPos,
            float marginX = 0f,
            float marginY = 0f)
        {
            if (camera == null) return false;

            Vector3 viewPos = camera.WorldToViewportPoint(worldPos);

            // 相机背面或裁剪平面外
            if (viewPos.z <= 0f) return false;
            if (viewPos.z < camera.nearClipPlane || viewPos.z > camera.farClipPlane) return false;

            // 边缘留白
            marginX = Mathf.Clamp01(marginX);
            marginY = Mathf.Clamp01(marginY);

            // 判断是否在视口范围内
            bool insideX = viewPos.x >= marginX && viewPos.x <= 1f - marginX;
            bool insideY = viewPos.y >= marginY && viewPos.y <= 1f - marginY;

            return insideX && insideY;
        }

        public static bool IsRendererVisible(this Camera camera, Renderer renderer)
        {
            if (renderer == null) return false;
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }


        #endregion

        #region UniTask関連

        private static readonly ConditionalWeakTable<Component, CancellationTokenSource> disableTokens = new();

        public static CancellationToken GetCancellationTokenOnDisable(this Component component)
        {
            if (!disableTokens.TryGetValue(component, out var source))
            {
                source = new CancellationTokenSource();
                disableTokens.Add(component, source);
                WatchDisable(component, source).Forget();
            }
            return source.Token;
        }

        private static async UniTaskVoid WatchDisable(Component component, CancellationTokenSource source)
        {
            var trigger = component.GetAsyncDisableTrigger();
            await trigger.OnDisableAsync();
            source.Cancel();
            disableTokens.Remove(component);
        }

        public static void WaitInput(this MonoBehaviour mono, ButtonControl buttonControl, UnityAction callback)
        {
            var token = mono.GetCancellationTokenOnDestroy();
            UniTask.Void(async (_) =>
            {
                await UniTask.Yield(PlayerLoopTiming.Update); // 确保进入 Update 循环
                while (!token.IsCancellationRequested && mono && mono.isActiveAndEnabled)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    if (buttonControl.wasPressedThisFrame)
                    {
                        callback?.Invoke();
                        break;
                    }
                }
            }, token);
        }

        public static void Delay(this float time,UnityAction callback,CancellationToken cancellationToken)
        {
            UniTask.Void(async (_) =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken:cancellationToken);
                callback?.Invoke();
            },cancellationToken);
        }

        public static void Delay(this MonoBehaviour mono, float time, UnityAction callback)
        {
            UniTask.Void(async () =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: mono.GetCancellationTokenOnDestroy());
                callback?.Invoke();
            });
        }

        #endregion

        #region BehaviorTree関連

        public static void SetProp<T>(this BehaviorTree tree, string propName, T value)
        {
            if (tree == null)
                return;

            var variable = tree.GetVariable(propName);
            if (variable == null)
                return;

            if (variable is SharedVariable<T> typedVar)
                typedVar.Value = value;
            else
                tree.SetVariableValue(propName, value);
        }

        public static T GetProp<T>(this BehaviorTree tree, string propName, T defaultValue = default)
        {
            if (tree == null)
                return defaultValue;

            var variable = tree.GetVariable(propName);
            if (variable == null)
                return defaultValue;

            try
            {
                if (variable is SharedVariable<T> typedVar)
                    return typedVar.Value;
                return (T)variable.GetValue();
            }
            catch
            {
                return defaultValue;
            }
        }

#endregion

        #region Animator関連
        /// <summary>
        /// 获取动画片段持续时间（秒）
        /// </summary>
        public static float GetClipDuration(this Animator animator, string clipName)
        {
            if (animator == null) return -1f;
            var clips = animator.runtimeAnimatorController.animationClips;
            var clip = clips.FirstOrDefault(c => c.name == clipName);
            return clip != null ? clip.length : -1f;
        }

        /// <summary>
        /// 获取当前状态持续时间（秒）
        /// </summary>
        public static float GetCurrentStateDuration(this Animator animator, int layer = 0)
        {
            if (animator == null) return -1f;
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            return info.length;
        }

        /// <summary>
        /// 获取当前动画状态名称
        /// </summary>
        public static string GetCurrentStateName(this Animator animator, int layer = 0)
        {
            if (animator == null) return null;
            var info = animator.GetCurrentAnimatorStateInfo(layer);
            return info.IsName("") ? null : GetStateName(animator, info.shortNameHash);
        }

        private static string GetStateName(Animator animator, int hash)
        {
            return hash.ToString();
        }

        #endregion
    }

}


