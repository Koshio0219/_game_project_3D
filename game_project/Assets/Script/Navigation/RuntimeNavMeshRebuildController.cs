using Game.Base;
using Game.Navigation;
using Game.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Navigation
{
    [DisallowMultipleComponent]
    public class RuntimeNavMeshRebuildController : MonoBehaviour
    {
        [Header("Builder")]
        public RuntimeNavMeshBuilder builder;   // 你的实时烘培脚本实例（要求 tracked = null）

        [Header("Update Frequency")]
        public float updateFrequency = 30f;  // 每秒更新频率

        [Header("Safe Start Point")]
        public Vector3Int safeStartPoint;

        public Dictionary<int, Enemy> mapEnemyIdToInstance; // 由你的关卡/敌人管理器维护与注入

        [Header("Bounds Settings")]
        [Tooltip("XZ 平面的外延距离")]
        public float padding = 4f;

        [Tooltip("Y 方向的体素高度（烘培体积高度）")]
        public float fixedHeight = 20f;

        [Tooltip("XZ 尺寸的最小值")]
        public Vector2 minSizeXZ = new(40, 40);

        [Tooltip("XZ 尺寸的最大值")]
        public Vector2 maxSizeXZ = new(100, 100);

        [Tooltip("当只有1个敌人时使用的默认尺寸（会在min/max里再夹一次）")]
        public Vector2 singleDefaultSizeXZ = new(50, 50);

        [Header("Smoothing")]
        [Tooltip("中心与尺寸的平滑因子，越大越跟手，越小越稳态")]
        public float followLerp = 8f;  // per second

        [Tooltip("尺寸变化过小则忽略（抖动阈值）")]
        public float sizeEpsilon = 0.25f;

        [Tooltip("中心变化过小则忽略（抖动阈值）")]
        public float centerEpsilon = 0.05f;

        // 内部缓存（平滑）
        private Vector3 _targetCenter;
        private Vector3 _targetSize;
        private bool _hasTarget = false;
        private float timer=999;

        private void Reset()
        {
            // 默认拿自身上的 Builder
            if (builder == null) builder = GetComponent<RuntimeNavMeshBuilder>();
        }

        private void Awake()
        {
            if (builder == null)
            {
                Debug.LogError("[NavRebuildController] Missing RuntimeNavMeshBuilder reference.");
                enabled = false;
                return;
            }
            // 关键：让控制器直接驱动中心
            builder.tracked = null;

            // 初始化
            _targetCenter = builder.transform.position;
            _targetSize = builder.size;
            _hasTarget = true;

            GameManager.runtimeNavMeshRebuildController = this;
        }

        private void OnDestroy()
        {
            GameManager.runtimeNavMeshRebuildController = null;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < updateFrequency) return;
            timer = 0f;

            if (builder == null) return;

            // 采集敌人坐标（你可以把 mapEnemyIdToInstance 由外部系统每帧更新或直接改成回调获取）
            var map = mapEnemyIdToInstance;

            Vector3 center;
            Vector3 size;

            bool ok;
            if (map != null && map.Count > 0)
            {
                // 使用计算器（注意：单一敌人时会回默认尺寸，你也可以强制 singleDefaultSizeXZ）
                ok = DynamicNavBounds.ComputeFromEnemies(
                    map,
                    padding,
                    fixedHeight,
                    minSizeXZ,
                    maxSizeXZ,
                    centerY: builder.transform.position.y, // 把中心Y固定在当前构建体的高度中心
                    out center,
                    out size
                );

                // 如果是单一目标，按你的默认尺寸再夹一次
                if (ok && map.Count == 1)
                {
                    size.x = Mathf.Clamp(singleDefaultSizeXZ.x, minSizeXZ.x, maxSizeXZ.x);
                    size.z = Mathf.Clamp(singleDefaultSizeXZ.y, minSizeXZ.y, maxSizeXZ.y);
                    size.y = fixedHeight;
                }
            }
            else
            {
                // 没有敌人：回到安全初始点
                builder.transform.position =  safeStartPoint;
                _ = builder.Build();
                return;
            }

            if (!ok) return;

            // 目标值
            if (!_hasTarget)
            {
                _targetCenter = center;
                _targetSize = size;
                _hasTarget = true;
            }
            else
            {
                _targetCenter = center;
                _targetSize = size;
            }

            // 平滑（避免抖动）
            float t = 1f - Mathf.Exp(-followLerp * Time.deltaTime); // 指数插值更稳
            Vector3 newCenter = Vector3.Lerp(builder.transform.position, _targetCenter, t);
            Vector3 newSize = Vector3.Lerp(builder.size, _targetSize, t);

            // 抖动阈值
            if ((newCenter - builder.transform.position).sqrMagnitude >= centerEpsilon * centerEpsilon)
                builder.transform.position = new Vector3(newCenter.x, builder.transform.position.y, newCenter.z);

            Vector2 sizeDeltaXZ = new(newSize.x - builder.size.x, newSize.z - builder.size.z);
            if (sizeDeltaXZ.sqrMagnitude >= sizeEpsilon * sizeEpsilon)
                builder.size = new Vector3(newSize.x, fixedHeight, newSize.z);

            _ = builder.Build();
        }
    }
}
