using UnityEngine;
using NavMeshAgent = UnityEngine.AI.NavMeshAgent;
using NavMesh = UnityEngine.AI.NavMesh;
using NavMeshHit = UnityEngine.AI.NavMeshHit;
using System.ComponentModel;
//using System;

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

        private NavMeshAgent agent;

        private TaskStatus status; 

        public override void OnAwake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            if (agent == null) return;
            status = TaskStatus.Running;
            agent.speed = speed.Value;
            //fix up the agent pos with the obj current pos
            agent.Warp(transform.position);
            DoWander();
        }

        public override TaskStatus OnUpdate()
        {
            if (agent ==null)
            {
                status = TaskStatus.Failure;
                return status;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + keepDistance.Value)
            {
                if (repeat)
                {
                    DoWander();
                }
                else
                {
                    status = TaskStatus.Success;
                }
            }

            return status;
        }

        void DoWander()
        {
            var min = minWanderDistance.Value;
            var max = maxWanderDistance.Value;
            min = Mathf.Clamp(min, 0.01f, max);
            max = Mathf.Clamp(max, min, max);
            var wanderPos = agent.transform.position;
            while ((wanderPos - agent.transform.position).sqrMagnitude < (min * min))
            {
                wanderPos = (Random.insideUnitSphere * max) + agent.transform.position;
            }

            if (NavMesh.SamplePosition(wanderPos, out NavMeshHit hit, float.PositiveInfinity, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
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
    }
}