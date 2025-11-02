using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.ComponentModel;

namespace Game.BehaviorTask
{
    [Description("Makes the agent wander randomly within the navigation map (robust to runtime baking).")]
    public class Wander : BehaviorDesigner.Runtime.Tasks.Action
    {
        [Header("Move")]
        public SharedFloat speed = 3;
        public SharedFloat keepDistance = .1f;

        [Header("Wander Radius")]
        public SharedFloat minWanderDistance = 5;   // 最小半径
        public SharedFloat maxWanderDistance = 8;   // 最大半径

        [Header("Behavior")]
        public bool repeat = true;                  // 达到后是否继续游走

        [Header("Picking")]
        public int maxPickTries = 8;            // 每次挑点最多尝试
        public float searchPadding = 2f;           // SamplePosition 的额外搜索半径

        [Header("Stuck Detection")]
        public float stuckTimeout = 3.0f;       // 认为卡住的时间（秒）
        public float progressEpsilon = 0.05f;      // remainingDistance 变化阈值（米）
        public float minSpeedToCount = 0.1f;       // 判定“没动”时的速度阈值（米/秒）

        [Header("Snap To NavMesh")]
        public float localSnapRadius = 2f;         // 近场吸附半径
        public float globalSnapRadius = 32f;        // 兜底吸附半径（有限值，避免AABB报错）

        private NavMeshAgent agent;
        private TaskStatus status;

        // 防卡进度追踪
        private float stuckTimer = 0f;
        private float lastRemaining = float.PositiveInfinity;

        public override void OnAwake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            if (agent == null || !agent.isActiveAndEnabled)
            {
                status = TaskStatus.Failure;
                return;
            }

            agent.speed = speed.Value;

            // 首帧先尝试吸附到网格（在运行时烘焙/瞬移后尤为重要）
            if (!AgentReady())
            {
                status = TaskStatus.Running; // 等下一帧再试
                return;
            }

            status = TaskStatus.Running;
            PickAndGo(); // 先挑一个可达点
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null || !agent.isActiveAndEnabled)
                return TaskStatus.Failure;

            // agent 可能因为实时烘焙/移动导致瞬间离网格
            if (!AgentReady())
                return TaskStatus.Running;

            // 还在算路径就先等
            if (agent.pathPending)
                return TaskStatus.Running;

            // 1) 到达：挑下一个或结束
            if (agent.hasPath &&
                agent.pathStatus == NavMeshPathStatus.PathComplete &&
                agent.remainingDistance <= agent.stoppingDistance + keepDistance.Value)
            {
                if (repeat) PickAndGo();
                else status = TaskStatus.Success;

                ResetStuck();
                return status;
            }

            // 2) 路径损坏（Partial/Invalid/无路径）：立刻重选
            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                PickAndGo();
                ResetStuck();
                return status;
            }

            // 3) 卡住判定：remainingDistance 几乎不变 & 速度很小
            float rem = agent.remainingDistance;
            float delta = Mathf.Abs(rem - lastRemaining);
            lastRemaining = rem;

            bool hardlyMoving = agent.velocity.sqrMagnitude < (minSpeedToCount * minSpeedToCount);
            if (delta < progressEpsilon && hardlyMoving)
                stuckTimer += Time.deltaTime;
            else
                stuckTimer = 0f;

            if (stuckTimer >= stuckTimeout)
            {
                // 认为卡住：重新挑点
                PickAndGo();
                ResetStuck();
            }

            return status;
        }

        public override void OnPause(bool paused)
        {
            if (paused) OnEnd();
        }

        public override void OnEnd()
        {
            if (AgentReady())
                SafeResetPath();

            ResetStuck();
        }

        // -------- Helpers --------

        private void ResetStuck()
        {
            stuckTimer = 0f;
            lastRemaining = float.PositiveInfinity;
        }

        /// <summary>
        /// 仅在“就绪”时才能安全访问/调用 Agent 的实例方法。
        /// 会尝试将 agent 吸附到最近的 NavMesh（近场→兜底）。
        /// </summary>
        private bool AgentReady()
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
            // 兜底吸附（有限半径！）
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
        /// 只有在就绪时才调用 ResetPath，避免报错。
        /// </summary>
        private void SafeResetPath()
        {
            if (agent == null || !agent.isActiveAndEnabled) return;
#if UNITY_2019_2_OR_NEWER
            if (!agent.isOnNavMesh) return;
#endif
            agent.ResetPath();
        }

        /// <summary>
        /// 选择可达目标并下发 SetDestination；失败则清路并保持 Running。
        /// </summary>
        private void PickAndGo()
        {
            if (agent == null || !agent.isActiveAndEnabled)
            {
                status = TaskStatus.Failure;
                return;
            }

            // 用 agent 的当前位置作为路径起点（无需读 remainingDistance 等）
            Vector3 origin = agent.transform.position;
            float y = origin.y;

            float min = Mathf.Max(0.01f, minWanderDistance.Value);
            float max = Mathf.Max(min, maxWanderDistance.Value);
            float searchRadius = Mathf.Max(max + Mathf.Max(0.01f, searchPadding), 2f);

            float curMax = max;
            var path = new NavMeshPath();

            for (int i = 0; i < Mathf.Max(1, maxPickTries); i++)
            {
                // 在XZ平面采样，避免高度偏差
                Vector2 o2 = Random.insideUnitCircle * curMax;
                if (o2.sqrMagnitude < (min * min))
                    continue;

                Vector3 candidate = new Vector3(origin.x + o2.x, y, origin.z + o2.y);

                // 将候选点投射到NavMesh（有限半径！）
                if (!NavMesh.SamplePosition(candidate, out var hit, searchRadius, agent.areaMask))
                {
                    // 更靠近自身再试：逐步收缩半径，远离边界
                    curMax = Mathf.Lerp(curMax, min, 0.4f);
                    continue;
                }

                // 关键：用静态API先校验可达（不依赖 agent 状态）
                if (NavMesh.CalculatePath(origin, hit.position, agent.areaMask, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    // 只有就绪时才真正下发目标
                    if (AgentReady())
                        agent.SetDestination(hit.position);
                    return;
                }

                // 不可达则继续尝试，顺带收缩半径
                curMax = Mathf.Lerp(curMax, min, 0.4f);
            }

            // 本轮挑不到可达点：就绪时清掉路径，下一帧再试
            if (AgentReady())
                SafeResetPath();

            status = TaskStatus.Running;
        }
    }
}
