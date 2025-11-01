using Game.Base;
using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyAi;

namespace Game.Action
{
    public class AnimationEventHandler : MonoBehaviour
    {
        // slime animation
        public void AlertObservers(string message)
        {
            Debug.Log($"Slime {message}");

            if (message.Equals("AnimationDamageEnded")
                || message.Equals("AnimationAttackEnded")
                || message.Equals("AnimationJumpEnded"))         
                ResetToIdle();
        }

        private void ResetToIdle()
        {
            var up = transform.GetRootParent();
            if (!up.TryGetComponent<IEnemyBaseAction>(out var enemy)) return;
            enemy.Idle();
        }
    }
}

