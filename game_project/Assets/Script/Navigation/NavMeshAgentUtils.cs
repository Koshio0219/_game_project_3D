using UnityEngine;
using UnityEngine.AI;

namespace Game.Navigation
{
    /// <summary>
    /// 通用的 NavMeshAgent 安全调用辅助工具：
    /// - 检查 Agent 是否在 NavMesh 上（AgentReady）
    /// - 尝试自动吸附到最近的网格（Warp）
    /// - 安全调用 ResetPath（SafeResetPath）
    /// - 静态路径可达性判断（CanReach）
    /// 适合在实时烘焙 / 动态移动 NavMeshSurface 的项目中使用
    /// </summary>
    public static class NavMeshAgentUtils
    {
        /// <summary>
        /// 检查 NavMeshAgent 是否有效并尝试吸附到 NavMesh。
        /// 若 agent 未放置在 NavMesh 上，则使用指定半径进行 SamplePosition + Warp。
        /// </summary>
        /// <param name="agent">目标 NavMeshAgent</param>
        /// <param name="localSnapRadius">近场吸附半径</param>
        /// <param name="globalSnapRadius">全局兜底吸附半径（有限值！不要用 Infinity）</param>
        /// <returns>是否已在 NavMesh 上</returns>
        public static bool AgentReady(NavMeshAgent agent, float localSnapRadius = 2f, float globalSnapRadius = 32f)
        {
            if (agent == null || !agent.isActiveAndEnabled)
                return false;

#if UNITY_2019_2_OR_NEWER
            if (agent.isOnNavMesh)
                return true;
#endif

            // 近场吸附
            if (NavMesh.SamplePosition(agent.transform.position, out var hit, localSnapRadius, agent.areaMask))
            {
                agent.Warp(hit.position);
#if UNITY_2019_2_OR_NEWER
                return agent.isOnNavMesh;
#else
                return true;
#endif
            }

            // 全局兜底吸附（有限半径）
            if (NavMesh.SamplePosition(agent.transform.position, out var hit2, globalSnapRadius, agent.areaMask))
            {
                agent.Warp(hit2.position);
#if UNITY_2019_2_OR_NEWER
                return agent.isOnNavMesh;
#else
                return true;
#endif
            }

            return false;
        }

        /// <summary>
        /// 只有当 Agent 处于激活且在 NavMesh 上时才调用 ResetPath。
        /// 可安全调用，无论 agent 是否有效。
        /// </summary>
        public static void SafeResetPath(NavMeshAgent agent)
        {
            if (agent == null || !agent.isActiveAndEnabled)
                return;

#if UNITY_2019_2_OR_NEWER
            if (!agent.isOnNavMesh)
                return;
#endif

            agent.ResetPath();
        }

        /// <summary>
        /// 静态路径可达性检测（不依赖 agent 是否在 NavMesh 上）。
        /// </summary>
        /// <param name="origin">起点位置</param>
        /// <param name="target">目标位置</param>
        /// <param name="areaMask">区域掩码（一般用 agent.areaMask）</param>
        /// <returns>若能计算出完整路径返回 true</returns>
        public static bool CanReach(Vector3 origin, Vector3 target, int areaMask = NavMesh.AllAreas)
        {
            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(origin, target, areaMask, path))
                return path.status == NavMeshPathStatus.PathComplete;
            return false;
        }
    }
}
