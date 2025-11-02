using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Game.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Tasks = BehaviorDesigner.Runtime.Tasks;

namespace Game.BehaviorTask
{
    public class MoveToGameObject : Tasks.Action
    {
        [RequiredField] public SharedGameObject target;

        [Header("Move")]
        public SharedFloat speed = 3f;
        public SharedFloat keepDistance = .1f;

        [Header("Agent Safety")]
        [Tasks.Tooltip("近场吸附半径")]
        public float localSnapRadius = 2f;
        [Tasks.Tooltip("兜底吸附半径（有限值！）")]
        public float globalSnapRadius = 32f;

        [Header("Repath")]
        [Tasks.Tooltip("目标点变化达到该距离时才重新下发路径")]
        public float repathEpsilon = 0.1f;

        [Tasks.Tooltip("若当前不可达，是否立刻返回 Failure（否则持续 Running 等待环境变化）")]
        public bool failIfUnreachable = false;

        private Vector3? lastRequest;
        private NavMeshAgent agent;
        private TaskStatus status;

        public override void OnAwake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            if (agent == null || target.Value == null)
            {
                status = TaskStatus.Failure;
                return;
            }

            agent.speed = speed.Value;

            // 安全：先确保 Agent 在 NavMesh 上（必要时会尝试吸附）
            if (!NavMeshAgentUtils.AgentReady(agent, localSnapRadius, globalSnapRadius))
            {
                status = TaskStatus.Running; // 等下一帧再试
                return;
            }

            status = TaskStatus.Running;
            TrySetDestinationIfNeeded(force: true);
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null || target.Value == null)
                return TaskStatus.Failure;

            // 随时可能因实时烘焙掉网格：未就绪时不要访问 agent 属性
            if (!NavMeshAgentUtils.AgentReady(agent, localSnapRadius, globalSnapRadius))
                return TaskStatus.Running;

            // 目标点变化则尝试重新设置目的地
            TrySetDestinationIfNeeded();

            // 还在算路径就等待
            if (agent.pathPending) return TaskStatus.Running;

            // 不可达或没路径
            if (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                if (failIfUnreachable) return TaskStatus.Failure;
                return TaskStatus.Running;
            }

            // 到达判定
            var tgt = target.Value.transform.position;
            if (Vector3.Distance(agent.transform.position, tgt) <= agent.stoppingDistance + keepDistance.Value)
            {
                status = TaskStatus.Success;
                return status;
            }

            return status;
        }

        public override void OnPause(bool paused)
        {
            if (paused) OnEnd();
        }

        public override void OnEnd()
        {
            // 只在就绪时清路径，避免报错
            if (NavMeshAgentUtils.AgentReady(agent, localSnapRadius, globalSnapRadius))
                NavMeshAgentUtils.SafeResetPath(agent);
        }

        public override void OnDrawGizmos()
        {
            if (target == null || target.Value == null) return;
            Gizmos.DrawWireSphere(target.Value.transform.position, keepDistance.Value);
        }

        // ---------------- Internal ----------------

        private void TrySetDestinationIfNeeded(bool force = false)
        {
            if (agent == null || target.Value == null) return;

            Vector3 pos = target.Value.transform.position;

            // 只有位置变化足够时才重新下发，避免频繁调用 SetDestination
            if (!force && lastRequest.HasValue)
            {
                if ((lastRequest.Value - pos).sqrMagnitude < repathEpsilon * repathEpsilon)
                    return;
            }

            // 先静态校验可达（不依赖 agent 是否在网格上）
            var origin = agent.transform.position;
            if (NavMeshAgentUtils.CanReach(origin, pos, agent.areaMask))
            {
                // 只有在就绪时才真正设置目的地
                if (NavMeshAgentUtils.AgentReady(agent, localSnapRadius, globalSnapRadius))
                    agent.SetDestination(pos);

                lastRequest = pos;
            }
            else
            {
                // 不可达：根据策略选择立即失败或持续等待
                if (failIfUnreachable)
                    status = TaskStatus.Failure;
                // 不更新 lastRequest，这样下一帧位置若变化会再次尝试
            }
        }
    }
}
