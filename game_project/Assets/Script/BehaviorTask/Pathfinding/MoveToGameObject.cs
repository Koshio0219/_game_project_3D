using Tasks= BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace Game.BehaviorTask
{
    public class MoveToGameObject : Tasks.Action
    {
        [RequiredField]
        public SharedGameObject target;
        public SharedFloat speed = 3;
        public SharedFloat keepDistance = .1f;

        private Vector3? lastRequest;
        private NavMeshAgent agent;

        private TaskStatus status;

        public override void OnAwake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            if (target.Value == null || agent == null) return;
            status = TaskStatus.Running;
            agent.speed = speed.Value;
            //fix up the agent pos with the obj current pos
            agent.Warp(transform.position);
        }

        public override TaskStatus OnUpdate()
        {
            if (target.Value == null || agent == null)
            {
                status = TaskStatus.Failure;
                return status;
            }

            if (Vector3.Distance(agent.transform.position, target.Value.transform.position) < agent.stoppingDistance + keepDistance.Value)
            {
                status = TaskStatus.Success;
                return status;
            }

            var pos = target.Value.transform.position;
            if (lastRequest != pos)
            {
                if (!agent.SetDestination(pos))
                {
                    status = TaskStatus.Failure;
                    return status;
                }
            }
            lastRequest = pos;
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.Value)
            {
                status = TaskStatus.Success;
                return status;
            }
            return status;
        }

        public override void OnPause(bool paused)
        {
            if (paused)
                OnEnd();
        }

        public override void OnEnd()
        {
            if (agent != null && agent.gameObject.activeSelf)
            {
                agent.Warp(agent.transform.position);
                agent.ResetPath();
            }
        }

        public override void OnDrawGizmos()
        {
            if (target.Value == null) return;
            Gizmos.DrawWireSphere(target.Value.transform.position, keepDistance.Value);
        }
    }
}