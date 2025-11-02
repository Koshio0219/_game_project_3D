using Game.Unit;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Navigation
{
    public static class DynamicNavBounds
    {
        /// <summary>
        /// 从敌人字典计算：中心点与大小（XZ 包围矩形 + padding，Y 高度固定）
        /// </summary>
        /// <param name="map">Dictionary&lt;int, Enemy&gt;（要求 Enemy 持有 Transform 或世界坐标）</param>
        /// <param name="padding">在 XZ 平面向外扩展的边距</param>
        /// <param name="fixedHeight">返回的 size.y 固定为该高度</param>
        /// <param name="minSizeXZ">最小 XZ 尺寸（避免过小）</param>
        /// <param name="maxSizeXZ">最大 XZ 尺寸（避免过大/过分发散）</param>
        /// <param name="centerY">返回的 center.y（一般用 NavMesh 体积构建的中点高度）</param>
        /// <param name="center">输出中心</param>
        /// <param name="size">输出大小</param>
        /// <returns>是否计算成功（有至少1个敌人）</returns>
        public static bool ComputeFromEnemies(
            Dictionary<int, Enemy> map,
            float padding,
            float fixedHeight,
            Vector2 minSizeXZ,
            Vector2 maxSizeXZ,
            float centerY,
            out Vector3 center,
            out Vector3 size)
        {
            center = default;
            size = default;

            if (map == null || map.Count == 0)
                return false;

            // 单一目标：直接返回目标点，使用“合理的默认大小”（夹在 min/max 内）
            if (map.Count == 1)
            {
                foreach (var kv in map)
                {
                    Vector3 p = kv.Value.transform.position;
                    center = new Vector3(p.x, centerY, p.z);
                    float sizeX = Mathf.Clamp(minSizeXZ.x, minSizeXZ.x, maxSizeXZ.x);
                    float sizeZ = Mathf.Clamp(minSizeXZ.y, minSizeXZ.y, maxSizeXZ.y);
                    size = new Vector3(sizeX, fixedHeight, sizeZ);
                    return true;
                }
            }

            // 多目标：计算 XZ AABB
            float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;

            foreach (var kv in map)
            {
                Vector3 p = kv.Value.transform.position;
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.z < minZ) minZ = p.z;
                if (p.z > maxZ) maxZ = p.z;
            }

            // 外延 padding
            minX -= padding; maxX += padding;
            minZ -= padding; maxZ += padding;

            // 中心与尺寸（XZ）
            float width = Mathf.Max(0.01f, maxX - minX);
            float depth = Mathf.Max(0.01f, maxZ - minZ);

            // 夹在一个适中的范围里
            width = Mathf.Clamp(width, minSizeXZ.x, maxSizeXZ.x);
            depth = Mathf.Clamp(depth, minSizeXZ.y, maxSizeXZ.y);

            // 中心点
            float cx = 0.5f * (minX + maxX);
            float cz = 0.5f * (minZ + maxZ);

            center = new Vector3(cx, centerY, cz);
            size = new Vector3(width, fixedHeight, depth);
            return true;
        }
    }
}
