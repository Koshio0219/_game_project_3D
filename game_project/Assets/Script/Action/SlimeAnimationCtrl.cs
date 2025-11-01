using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Action
{
    public class SlimeAnimationCtrl : MonoBehaviour
    {
        public Animator animator;
        public NavMeshAgent agent;

        void OnAnimatorMove()
        {
            // apply root motion
            Vector3 Anipos = animator.rootPosition.FixHeight(agent.nextPosition.y);
            agent.transform.position = Anipos;
            agent.nextPosition = agent.transform.position;
        }
    }
}

