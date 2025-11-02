using UnityEngine;
using UnityEngine.AI;
using System.ComponentModel;
using Tasks = BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Game.BehaviorTask
{
    [Description("Makes the agent wander randomly within the navigation map")]
    public class Wander : Tasks.Action
    {
        public SharedFloat speed = 3;
        public SharedFloat keepDistance = .1f;
        public SharedFloat minWanderDistance = 5;
        public SharedFloat maxWanderDistance = 8;
        public bool repeat = true;

        // 失败时最大重试次数
        [Description("How many times to try sampling a valid wander point this tick")]
        public int maxTriesPerTick = 6;

        private NavMeshAgent agent;
        private TaskStatus status;

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

            // 关键：首帧优先确保落在NavMesh上，再考虑寻路
            if (!EnsureAgentPlaced(2f))
            {
                // 还没落上，先进入Running，下一帧再尝试
                status = TaskStatus.Running;
                return;
            }

            status = TaskStatus.Running;
            DoWander();
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null || !agent.isActiveAndEnabled)
                return TaskStatus.Failure;

            // 未落到NavMesh上时，别碰remainingDistance，先尝试吸附
            if (!EnsureAgentPlaced(2f))
                return TaskStatus.Running;

            // 只有在Agent已就绪时才读这些属性
            if (!agent.pathPending && agent.hasPath &&
                agent.remainingDistance <= agent.stoppingDistance + keepDistance.Value)
            {
                if (repeat)
                    DoWander();
                else
                    status = TaskStatus.Success;
            }

            return status;
        }

        void DoWander()
        {
            if (agent == null || !agent.isActiveAndEnabled)
            {
                status = TaskStatus.Failure;
                return;
            }

            // 计算采样参数（都要是有限值）
            float min = Mathf.Max(0.01f, minWanderDistance.Value);
            float max = Mathf.Max(min, maxWanderDistance.Value);
            float searchRadius = Mathf.Max(max + 1.0f, 2.0f); // 给SamplePosition用的有限半径（不要Infinity）

            Vector3 origin = agent.transform.position;
            float y = origin.y;

            // 限次尝试，避免死循环
            for (int i = 0; i < maxTriesPerTick; i++)
            {
                // 在XZ平面采样，避免把y抬很高或压很低
                Vector2 offset2D = Random.insideUnitCircle * max;
                // 至少要离当前点min距离
                if (offset2D.sqrMagnitude < (min * min))
                {
                    // 太近就再试
                    continue;
                }

                Vector3 candidate = new Vector3(origin.x + offset2D.x, y, origin.z + offset2D.y);

                // 用有限半径 + areaMask 采样最近可走点
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, searchRadius, agent.areaMask))
                {
                    // 可选：再验证一下路径是否可达
                    NavMeshPath path = new NavMeshPath();
                    if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(hit.position);
                        return;
                    }
                }
            }

            // 走到这里说明这帧没采样到合适点：保留Running，下帧再试；或根据需求Fail
            // status = TaskStatus.Failure;
            // 这里选择保持Running，避免行为树直接失败
        }

        public override void OnPause(bool paused)
        {
            if (paused)
                OnEnd();
        }

        public override void OnEnd()
        {
            if (agent != null && agent.isActiveAndEnabled && agent.gameObject.activeInHierarchy)
            {
                agent.ResetPath();
                // 不再Warp到自身位置，避免在边界处重复Warp造成奇怪状态
            }
        }

        // 有些Unity版本没有NavMeshAgent.isOnNavMesh属性，做个反射/编译期兼容判断
        bool HasIsOnNavMesh()
        {
#if UNITY_2019_2_OR_NEWER
            return true;
#else
            return false;
#endif
        }

        // 添加：统一的就绪判定与吸附
        bool EnsureAgentPlaced(float snapRadius = 2f)
        {
            if (agent == null || !agent.isActiveAndEnabled) return false;

#if UNITY_2019_2_OR_NEWER
            if (agent.isOnNavMesh) return true;
#endif

            // 尝试把当前Transform位置吸附到最近网格
            if (NavMesh.SamplePosition(transform.position, out var hit, snapRadius, agent.areaMask))
            {
                agent.Warp(hit.position);
#if UNITY_2019_2_OR_NEWER
                return agent.isOnNavMesh;
#else
        return true;
#endif
            }
            return false;
        }

    }
}
